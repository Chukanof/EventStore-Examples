/// <reference path="jasmine/jasmine.js"/>
/// <reference path="../MeasurementReadCount.js"/>

describe("when counting measurement reads", function () {
    
    var counter;
    var defaultEvent;

    beforeEach(function () {
        defaultEvent = { body: {} };
        counter = eventCounter();
    });

    it("should initialize new state on first event", function () {

        var initState = counter.init();

        expect(initState.count).toEqual(0);
    });

    it("should count the number of events", function () {

        var state = counter.init();
        state = counter.increase(state, defaultEvent);
        state = counter.increase(state, defaultEvent);
        state = counter.increase(state, defaultEvent);

        expect(state.count).toEqual(3);
    });
});