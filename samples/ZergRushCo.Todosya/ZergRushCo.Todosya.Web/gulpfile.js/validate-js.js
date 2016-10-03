/*
 * Validates application .js code with jshint and eslint.
 *
 * 09/30/2016 [Ivan Kozhin] Initial version.
 */

var gulp = require('gulp'),
    path = require('path'),
    config = require('../config.js');

gulp.task('validate-js', function () {
    var jshint = require('gulp-jshint');
        eslint = require('gulp-eslint');
    empty.reporter = empty.format = empty.failAfterError = empty.formatter = empty;
    return gulp.src('./**/*.js', { cwd: config.src })
        .pipe(jshint())
        .pipe(jshint.reporter('default'))
        .pipe(eslint())
        .pipe(eslint.format())
        .pipe(eslint.failAfterError());
});
