/*
 * Copy .js from pages to bundle directory with additional processing.
 * Can be used with minify and babel.
 *
 * 09/30/2016 [Ivan Kozhin] Initial version.
 */

var gulp = require('gulp'),
    path = require('path'),
    sourcemaps = require('gulp-sourcemaps'),
    gulpif = require('gulp-if'),
    uglify = require('gulp-uglify'),
    config = require('../config.js');

gulp.task('bundle-app-pages', function () {
    var babel = config.useBabel ? require('gulp-babel') : empty;
    return gulp.src(path.join(config.src, config.pagesDir, '**/*.js'))
        .pipe(gulpif(config.useSourcemaps, sourcemaps.init()))
        .pipe(gulpif(config.useBabel, babel({ presets: ['es2015'] })))
        .pipe(gulpif(config.production, uglify()))
        .pipe(gulpif(config.useSourcemaps, sourcemaps.write()))
        .pipe(gulp.dest(path.join(config.dest, config.pagesDir)));
});

// empty pipeline item
function empty () {
    var through = require('through2');
    return through.obj(function (file, enc, cb) {
        cb(null, file);
    });
}
