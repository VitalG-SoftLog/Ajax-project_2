var InheritOneClassFromAnother = function() { };

InheritOneClassFromAnother.prototype.Standart = function(Child, Parent) {
    var F = function() { }
    F.prototype = Parent.prototype;
    Child.prototype = new F();
    Child.prototype.constructor = Child;
    Child.baseClass = Parent.prototype;
};

InheritOneClassFromAnother.prototype.Extended = function(Child, Parent) {
    var F = function() { }
    F.prototype = Parent.prototype;
    Child.prototype = new F();
    Child.prototype.constructor = Child;
    Child.baseConstructor = Parent;
    Child.baseClass = Parent.prototype;
};

if (typeof ClassInheritance == "undefined") {
    ClassInheritance = new InheritOneClassFromAnother();
}
	