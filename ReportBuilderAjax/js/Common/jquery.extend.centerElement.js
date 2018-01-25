//Use window in selector
jQuery.fn.getPageSize = function() {
    this.Window = this[0];
    var xScroll, yScroll;
    if (this.Window.innerHeight && this.Window.scrollMaxY) {
        xScroll = this.Window.document.body.scrollWidth;
        yScroll = this.Window.innerHeight + this.Window.scrollMaxY;
    } else if (this.Window.document.body.scrollHeight > this.Window.document.body.offsetHeight) { // all but Explorer Mac
        xScroll = this.Window.document.body.scrollWidth;
        yScroll = this.Window.document.body.scrollHeight;
    } else if (this.Window.document.documentElement && this.Window.document.documentElement.scrollHeight > this.Window.document.documentElement.offsetHeight) { // Explorer 6 strict mode
        xScroll = this.Window.document.documentElement.scrollWidth;
        yScroll = this.Window.document.documentElement.scrollHeight;
    } else { // Explorer Mac...would also work in Mozilla and Safari
        xScroll = this.Window.document.body.offsetWidth;
        yScroll = this.Window.document.body.offsetHeight;
    }
    var windowWidth, windowHeight;
    if (this.Window.self.innerHeight) { // all except Explorer
        windowWidth = this.Window.self.innerWidth;
        windowHeight = this.Window.self.innerHeight;
    } else if (this.Window.document.documentElement && this.Window.document.documentElement.clientHeight) { // Explorer 6 Strict Mode
        windowWidth = this.Window.document.documentElement.clientWidth;
        windowHeight = this.Window.document.documentElement.clientHeight;
    } else if (document.body) { // other Explorers
        windowWidth = this.Window.document.body.clientWidth;
        windowHeight = this.Window.document.body.clientHeight;
    }
    // for small pages with total height less then height of the viewport
    if (yScroll < windowHeight) {
        pageHeight = windowHeight;
    } else {
        pageHeight = yScroll;
    }
    // for small pages with total width less then width of the viewport
    if (xScroll < windowWidth) {
        pageWidth = windowWidth;
    } else {
        pageWidth = xScroll;
    }
    return { pageWidth: pageWidth, pageHeight: pageHeight, windowWidth: windowWidth, windowHeight: windowHeight };
}

//Use window in selector
jQuery.fn.getWindowScrollTop = function() {
    var sender = this[0];
    return sender.self.pageYOffset || (sender.document.documentElement && sender.document.documentElement.scrollTop) || (sender.document.body && sender.document.body.scrollTop);
}

jQuery.fn.centerInFrame = function(iFrameIndex) {
    if (!iFrameIndex) iFrameIndex = 0;
    var pageSize = $(window.parent).getPageSize();
    var scrollY = $(window.parent).getWindowScrollTop();
    var iframeTop = window.parent.document.body.getElementsByTagName("iframe")[iFrameIndex].offsetTop;
    this.css("position", "absolute");
    this.css("top", (pageSize.windowHeight - this.height()) / 2 + scrollY - iframeTop + "px");
    this.css("left", ($(window).width() - this.width()) / 2 + 0 + "px");
    return this;
}

jQuery.fn.center = function() {
    this.css("position", "absolute");
    this.css("top", ($(window).height() - this.height()) / 2 + $(window).scrollTop() + "px");
    this.css("left", ($(window).width() - this.width()) / 2 + $(window).scrollLeft() + "px");
    return this;
}