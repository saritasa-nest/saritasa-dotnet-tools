/*
 * Concatenate .js frontend files. Can be used with minify and babel.
 *
 * 09/30/2016 [Ivan Kozhin] Initial version.
 * 12/14/2016 [Ivan Kozhin] Formatting.
 */

var gulp = require('gulp'),
    path = require('path'),
    sourcemaps = require('gulp-sourcemaps'),
    concat = require('gulp-concat'),
    gulpif = require('gulp-if'),
    config = require('../config.js');

gulp.task('bundle-app', function () {
    var babel = config.useBabel ? require('gulp-babel') : empty;
    return gulp
        .src(path.join(config.src, '**/*.js'), {
            ignore: './' + path.join(config.src, config.pagesDir, '**/*.js')
        })
        .pipe(gulpif(config.useSourcemaps, sourcemaps.init()))
        .pipe(gulpif(config.useBabel, babel({ presets: ['es2015'] })))
        .pipe(gulpif(config.production, uglify()))
        .pipe(concat(config.destName + '.js'))
        .pipe(gulpif(config.useSourcemaps, sourcemaps.write()))
        .pipe(gulp.dest(path.join(config.dest, config.pagesDir)));
});
