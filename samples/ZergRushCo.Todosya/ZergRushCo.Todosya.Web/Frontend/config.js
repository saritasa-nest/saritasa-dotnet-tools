'use strict';

requirejs.config({
    urlArgs: 'bust=' + (new Date()).getTime(),
    paths: {
        'domReady': '../bower_components/domReady/domReady',
        'jquery': '../bower_components/jquery/dist/jquery',
        'knockout': '../bower_components/knockout/dist/knockout',
        'tinyColorPicker': '../bower_components/tinyColorPicker/jqColorPicker',
        'colors': '../bower_components/tinyColorPicker/colors'
    },
    map: {
        '*': {
            'jquery': 'jquery-noconflict'
        },
        'jquery-noconflict': {
            'jquery': 'jquery'
        },
        'tinyColorPicker': {
            deps: ['jquery']
        }
    }
});
