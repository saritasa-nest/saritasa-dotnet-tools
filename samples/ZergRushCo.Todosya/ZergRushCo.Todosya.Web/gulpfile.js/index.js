'use strict';

// config setup
var config = require('../config.js');
config.production = process.argv.filter(a => a.match(/(--)?production/) !== null).length > 0 ? true : config.production;
config.useSourcemaps = process.argv.filter(a => a.match(/(--)?sourcemaps/) !== null).length > 0 ? true : config.useSourcemaps;
config.debug = process.argv.filter(a => a.match(/(--)?debug/) !== null).length > 0 ? true : config.debug;

// load other tasks
var requireDir = require('require-dir');
requireDir('.', { recurse: true });

var gulp = require('gulp'),
    runSequence = require('run-sequence'),
    path = require('path'),
    glob = require('glob'),
    concat = require('gulp-concat'),
    gulpif = require('gulp-if'),
    rename = require('gulp-rename'),
    replace = require('gulp-replace');

// pre-create bundles directory
let fs = require('fs');
if (!fs.existsSync(config.dest)) {
    fs.mkdirSync(config.dest);
}

// default is to bundle everything
gulp.task('default', function () {
    runSequence(
        config.useRequireJS ? 'bundle-app-requirejs' : 'bundle-app',
        'bundle-bower-style',
        'bundle-app-pages',
        'bundle-style',
        'clean-temp');
});
