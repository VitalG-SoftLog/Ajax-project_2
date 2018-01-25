
(function($)
{
    var getDocumentScrollLeft = function(doc)
    {
        doc = doc || document;
        return Math.max(doc.documentElement.scrollLeft, doc.body.scrollLeft);
    }

    var getDocumentScrollTop = function(doc)
    {
        doc = doc || document;
        return Math.max(doc.documentElement.scrollTop, doc.body.scrollTop);
    }

    jQuery.Region = 
    {
        getElementLocation : function(el)
        {
            var elementLocation = {top:0, left:0};

            if (document.documentElement.getBoundingClientRect)  // IE
            {
                    var box = el.getBoundingClientRect();

                    var rootNode = el.ownerDocument;
                                
                    elementLocation.left = box.left + getDocumentScrollLeft(rootNode);
                    elementLocation.top = box.top + getDocumentScrollTop(rootNode);
            }
            else
            {
                    // manually calculate by crawling up offsetParents
                    var pos = [el.offsetLeft, el.offsetTop];
                    var parentNode = el.offsetParent;

                    // safari: subtract body offsets if el is abs (or any offsetParent), unless body is offsetParent
                    var accountForBody = ($.browser.safari &&
                            el.style.position == 'absolute' &&
                            el.offsetParent == el.ownerDocument.body);

                    if (parentNode != el) 
                    {
                        while (parentNode) 
                        {
                            pos[0] += parentNode.offsetLeft;
                            pos[1] += parentNode.offsetTop;
                            if (!accountForBody && $.browser.safari && 
                                    parentNode.style.position == 'absolute' ) 
                                    { 
                                        accountForBody = true;
                                    }
                            parentNode = parentNode.offsetParent;
                        }
                    }

                    if (accountForBody) //safari doubles in this case
                    { 
                        pos[0] -= el.ownerDocument.body.offsetLeft;
                        pos[1] -= el.ownerDocument.body.offsetTop;
                    } 
                    parentNode = el.parentNode;

                    // account for any scrolled ancestors
                    while ( parentNode.tagName && !(/^body|html$/i).test(parentNode.tagName) ) 
                    {
                       // work around opera inline/table scrollLeft/Top bug
                       if (parentNode.style.display.search(/^inline|table-row.*$/i)) 
                       { 
                            pos[0] -= parentNode.scrollLeft;
                            pos[1] -= parentNode.scrollTop;
                       }
                        
                       parentNode = parentNode.parentNode; 
                    }
                    
                    elementLocation.left = pos[0];
                    elementLocation.top = pos[1];
            }

            return elementLocation;
        },
       
        getBounds : function(el)
        {
            var p = jQuery.Region.getElementLocation(el);
            
            var elementBounds = {left: 0, top: 0, width: 0, height: 0};
            elementBounds.left = p.left;
            elementBounds.top = p.top;
            elementBounds.width = el.offsetWidth;
            elementBounds.height = el.offsetHeight;
            
            return elementBounds;
        }
    };
})(jQuery);