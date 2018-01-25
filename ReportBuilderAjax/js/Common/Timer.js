/*
 * @description This object fire callback function with interval
 * @class Timer
 * @param {Function} callbackFunction
 * @param {Number} interval
 */
function Timer(callbackFunction, interval) {
	/*
	 * @description The number of milliseconds (thousandths of a second) that the function call should be delayed by
	 * @property Interval
	 * @type Number
	 */
    this.Interval = interval;
    
	/*
	 * @description The function you want to execute after delay milliseconds.
	 * @property Callback
	 * @type Function
	 */    
    this.Callback = callbackFunction;
    
	/*
	 * @description The ID of the timeout, which can be used with window.clearTimeout.
	 * @property Timer
	 * @type Object
	 */     
    this.Timer = null;
};

/*
 * @method startTimer
 * @description Clear old timer and create new
 */
Timer.prototype.startTimer = function() {
    this.stopTimer();
    this.Timer = window.setTimeout(this.Callback, this.Interval);
};

/*
 * @method startTimer
 * @description Clear old timer
 */
Timer.prototype.stopTimer = function() {
        if(this.Timer !== null) {
            window.clearTimeout(this.Timer);
            this.Timer = null;
        }
};