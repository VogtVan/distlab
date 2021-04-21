# 0.3.3
- fixed rendering of SVG which are higher than wide

# 0.3.1
- Package update due to security vulnerabilities.

# 0.3.0
- Implemented support for data in docs type. (e.g. Elasticsearch Raw Document) Thanks to Zoltán Szabó (https://github.com/zoell)
- The data passed to the panel is now stored in the `ctrl.data` property. The alias property `ctrl.series` should not be used anymore and is to be regarded deprecated. 
 
 # 0.2.0
- Implemented support for data in table format. Thanks to Lauri Saurus (https://github.com/saurla)

# 0.1.1
- now includes JavaScript code completion for the objects this, ctrl and svgnode.

![Screenshot](https://raw.githubusercontent.com/MarcusCalidus/marcuscalidus-svg-panel/master/dist/img/codeCompletion_0.1.1.png)

# 0.1.0
- BREAKING: plugin was renamed marcuscalidus-svg-panel in line with http://docs.grafana.org/plugins/developing/code-styleguide/

 ### Steps to update from older version
 
 * export dashboard containing grafana-svg-panel as json
 * install marcuscalidus-svg-panel plugin. Either by cloning it into separate folder (=safe method) or by pulling it into the current version.
 * replace all occurrences of grafana-svg-panel with marcuscalidus-svg-panel in the json file.
 * reimport the json to overwrite the existing dashboard

# 0.0.5
- ace editor for code and SVG editing
## 0.0.4
- panel now runs smoothly in IE11 (added necessary polyfill)
## 0.0.3
- fixed bug with onInit function in Grafana 5
- new method for injecting SVG via Snap svg library
## 0.0.2
### New features
- SVG Builder to compose your own complex SVG dashboard
## 0.0.1
Initial release
