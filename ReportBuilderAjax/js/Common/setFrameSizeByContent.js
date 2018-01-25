function setIFrameSizeByCurrentContent() {
    try {
        var oFrame = window.parent.document.body.getElementsByTagName("iframe")[0];
        $(oFrame).height(window.document.body.clientHeight * 1 + oFrame.offsetTop * 1 + 50);
    }
    catch (e) {
        window.status = 'Error: ' + e.number + '; ' + e.description;
    }
}
