/*
 * Validates application .css with csslint.
 *
 * 09/30/2016 [Ivan Kozhin] Initial version.
 * 12/14/2016 [Ivan Kozhin] Formatting.
 */

var gulp = require('gulp'),
    path = require('path'),
    config = require('../config.js');

gulp.task('validate-style', function () {
    var csslint = require('gulp-csslint');
    return gulp
        .src('./**/*.css', { cwd: config.src })
        .pipe(csslint())
        .pipe(csslint.formatter('fail'));
});
