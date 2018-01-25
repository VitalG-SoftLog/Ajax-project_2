if (typeof TemplateEngine == "undefined") {
    /*
     * The TemplateEngine global namespace object.  If TemplateEngine is already defined, the
     * existing TemplateEngine object will not be overwritten so that defined
     * namespaces are preserved.
     * @class TemplateEngine
     */
    var TemplateEngine = {};
}

/*
 * @method Format
 * @description Replase in string all attributes
 * @param {String} template 
 * @param {Array(key, value)} attributes 
 */
TemplateEngine.Format = function(template, attributes) {
    var result = template;
    for (var attrName in attributes) {
        re = new RegExp();
        re.compile('{#' + attrName + '}', "g");
        result = result.replace(re, attributes[attrName]);
    }
    re = new RegExp();
    re.compile('ampersand;', "g");
    result = result.replace(re, "&"); //Ice hack for accept using '&' in templates
    return result;
};

/*
TemplateEngine.Format = function(template, attributes) {
	for(var i=0; i<attributes.length; i++) {
		template = template.replace('{#' + i + '}', attributes[i]);
	}
	return template;
};
*/	