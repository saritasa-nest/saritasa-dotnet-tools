/*
 * Copy .html from pages to bundle directory with additional processing.
 * Can be used with minify.
 *
 * 09/30/2016 [Ivan Kozhin] Initial version.
 * 12/14/2016 [Ivan Kozhin] Formatting.
 * 12/18/2016 [Ivan Kozhin] Actual html processing.
 */

var gulp = require('gulp'),
    path = require('path'),
    gulpif = require('gulp-if'),
    config = require('../config.js');

gulp.task('bundle-app-pages', function () {
    var htmlmin = config.useHtmlMin ? require('gulp-htmlmin') : empty;

    return gulp
        .src(path.join(config.src, config.pagesDir, '**/*.html'))
        .pipe(gulpif(config.production, htmlmin({collapseWhitespace: true})))
        .pipe(gulp.dest(path.join(config.dest, config.pagesDir)));
});

// empty pipeline item
function empty() {
    var through = require('through2');
    return through.obj(function (file, enc, cb) {
        cb(null, file);
    });
}
