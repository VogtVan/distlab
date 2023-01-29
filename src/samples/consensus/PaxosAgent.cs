using System.Threading.Tasks;
using System.Diagnostics;
using distlab.metrics;
using distlab.core;
using distlab.model;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using distlab.runtime.container;

namespace distlab.samples.consensus{

    public class PaxosConfig: ContainerConfigDefinition{
        // distinguished proposer and learner
        public bool IsLeader{get;set;}
        public int MinDelayMs{get;set;}
        public int MaxDelayMs{get;set;}
        public int StartingProposalNumber{get;set;}

    }
    // Paxos Made Simple - Leslie Lamport - 2011
    //
    // SAFETY:
    // Only a value that has been proposed may be chosen,
    // Only a single value is chosen, 
    // A process never learns that a value has been chosen unless it actually has been.
    //
    // LIVENESS- let us ensure that:
    // a proposed value will eventually be chosen
    // if a value is chosen, it will eventually be learnt
    //
    public class PaxosAgent : Container<PaxosConfig>{
        // an acceptor must accept the first proposal it receives
        // an acceptor must accept more than one proposal
        // a proposal should have a unique natural number (could be implemented with a range of numbers by proposers ?)
        // a proposal is accepted when a majority of acceptor have accepted the value => number of acceptors must be known

        // we can allow multiple proposals to be chosen if they have the same value 
        // => if a proposal is chosen with value v, every other proposal of heigher number that is chosen has value v [ couldn't we accept more than one v, if the proposer is different for instance ?]
        // => if a proposal is chosen with value v, every other proposal  of heigher number issued by any proposer has value v [seems weird and useless to contine to propose anything, and how to respect that ?]
        // need to maintain an invariance by controlling the future with a prepare request
        // PROPOSER.prepare(n) => ACCEPTOR (majority of)
        // if not already ok for p>n, ACCEPTOR.prepare_ok(n, lessThanNAlreadyAcceptedIfAny) => PROPOSER
        // if/when majority accepted PROPOSER.accept(n, proposal( max(lessThanNAlreadyAcceptedIfAny)) | any v ) => ACCEPTOR (the set of acceptor can be different than previous one)
        // if has not aked a prepared a proposal > n ACCEPTOR.accept_ok => PROPOSER
        //
        // learner may be informed by all acceptors when the accept_ok is sent back, need the majority here, couldn't it be the proposer which could be the distinguished learner ?
        // acceptor needs to remmember and serialize: highest accepted, highest prepare responded
        // progress guaranteed if there is a ditinguished proposer: random election or timeout ?
        //
        // PROBLEMS:
        // => choosing a leader
        // => ordering numbers: different set of numbers ok, but need to accept n and proposer Id. This will eventually make a proposer know another one is alive.
        //
        // REMARKS:
        // A single proposer may initiate a single prepare phase and then issue multiple accepts until another leader emerges
        // Because the distinguished proposer may crash, it needs to be elected
        // When the distinguished proposer has crashed is unknown => need to propose everytime; when a node is not proposed anything, at a random time it may try to start to propose 
        Random rnd = new Random();
        protected override async Task main() {
            if(this.Definition[Index].IsLeader){
                //tries first to get promises
                int value = await getPromises(rnd.Next(0, 100));
                //can now send accept requests
            }
        }

        async Task<int> getPromises(int value){
            (bool promised, int? value) proposalResult = (false, null);
            while(!proposalResult.promised){
                await Task.Delay(rnd.Next(this.Definition[Index].MinDelayMs, this.Definition[Index].MaxDelayMs));
                // starting the root activity for tracing
                Activity activity = this.startActivity($"prepare {value} for proposal number {this.Definition[Index].StartingProposalNumber}");
                try{
                    proposalResult = await propose(new Proposal{Number=this.Definition[Index].StartingProposalNumber, Value=value});
                    return proposalResult.value ?? value;
                }
                finally{
                    activity?.Dispose();
                }
            }
        }
        
        async Task<(bool,int?)> propose(Proposal proposal){
            logger.LogInformation("Proposer {Index} proposing {proposal}", Index, proposal);
            try{
                // For the time being sends prepare to everyone (could be any majority)
                IEnumerable<int> targets = Enumerable.Range(0, this.Definition.Instances);
                Task<PrepareResult>[] tasks = targets.Select(i => call<PrepareResult>(ServiceName, "prepare", i, proposal)).ToArray();
                // Must be serialized
                this.Definition[Index].StartingProposalNumber++;
                await Task.WhenAll(tasks);
                // Extracts the results and see if a majority promised    
                 bool proposalPromised = tasks.Count(t => t.Result.CurrentPromised) > (int)this.Definition.Instances/2;
                IEnumerable<Proposal> proposals = tasks.Select(t => t.Result.PreviousProposalAccepted).Where(p => p !=null);
                logger.LogInformation("Proposer {Index} - proposal {proposal} promised: {proposalPromised}", Index, proposal, proposalPromised);
                int? value = null;
                int? max = null;
                if(proposals.Count()>0){
                    max = proposals.Max(p => p.Number);
                    value = proposals.Where(p => p.Number==max).First().Value;  
                    logger.LogInformation("Proposer {Index} - promises: highest accepted {max} - value {value}", Index, max, value);
                }
                return (proposalPromised, value);    
            }
            catch(Exception e){
                logger.LogError("Unable to propose: {exception}.", e.Message);
                return (false, null);
            }
        }

        public class Proposal{            
            public int Number{get;set;}
            public int Value{get;set;}
            public override string ToString() => $"[Number: {Number} - Value: {Value}]";

        }
        
        public class PrepareResult{
            public Proposal PreviousProposalAccepted{get;set;}

            public bool CurrentPromised{get;set;}

            public override string ToString() => $"[PreviousProposalAccepted: {(PreviousProposalAccepted !=null ? PreviousProposalAccepted.ToString() : string.Empty)} - CurrentPromised: {CurrentPromised}";
        }
        
        // Must be serialized
        Proposal accepted = null;
        Proposal promised = null;
        readonly object promiseLocker = new object();
        readonly object acceptedLocker = new object(); 
        public async Task<PrepareResult> prepare(Proposal proposal){
            // never promised anything or newer proposal incoming
            // here we respond ok even if we have already promised for the same proposal (no optimisation)
            lock(promiseLocker){
                if(promised==null || promised.Number<=proposal.Number){
                    // remember the promise
                    promised = proposal;
                    // TODO serialize
                    logger.LogInformation("Acceptor {Index} promised {proposal}", Index, proposal);
                    lock(acceptedLocker)// not sure it is worth it
                        return new PrepareResult{PreviousProposalAccepted=accepted, CurrentPromised=true};
                }
                else
                    // we prefer here to return a nack rather than causing a timeout (no optimisation)
                    return new PrepareResult{CurrentPromised=false};
            }
        }
        
        public async Task<bool> accept(Proposal proposal){
            lock(promiseLocker){
                // accept unless one have promised for a higher proposal
                if(promised==null || promised.Number<=proposal.Number){
                    lock(acceptedLocker)
                        // remember the accept
                        accepted = proposal;
                    // TODO serialize
                    logger.LogInformation("Acceptor {Index} accepted {proposal}", Index, proposal);
                    return true;
                }
                else
                    return false;
            }
        }

    }
}