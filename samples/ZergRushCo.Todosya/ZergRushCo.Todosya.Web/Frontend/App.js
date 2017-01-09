'use strict';

/*
 * Main application module. Loads with every page load.
 */

define(function () {
    var App = {};

    App.sayHello = function () {
        console.log('Hello');
    };

    App.contentTypeJson = 'application/json';

    return App;
});
