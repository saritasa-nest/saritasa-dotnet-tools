/*
 * Validates application .js code with jshint and eslint.
 *
 * 09/30/2016 [Ivan Kozhin] Initial version.
 * 12/14/2016 [Ivan Kozhin] Formatting.
 * 12/14/2016 [Ivan Kozhin] Fix "empty" error.
 */

var gulp = require('gulp'),
    path = require('path'),
    config = require('../config.js');

gulp.task('validate-js', function () {
    var jshint = require('gulp-jshint'),
        eslint = require('gulp-eslint');
    empty.reporter = empty.format = empty.failAfterError = empty.formatter = empty;
    return gulp
        .src('./**/*.js', { cwd: config.src })
        .pipe(jshint())
        .pipe(jshint.reporter('default'))
        .pipe(eslint())
        .pipe(eslint.format())
        .pipe(eslint.failAfterError());
});

// empty pipeline item
function empty() {
    var through = require('through2');
    return through.obj(function (file, enc, cb) {
        cb(null, file);
    });
}
