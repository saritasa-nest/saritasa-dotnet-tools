/*
 * Concatenate .scss|.css frontend files.
 *
 * 09/30/2016 [Ivan Kozhin] Initial version.
 * 12/14/2016 [Ivan Kozhin] Formatting.
 */

var gulp = require('gulp'),
    path = require('path'),
    sourcemaps = require('gulp-sourcemaps'),
    concat = require('gulp-concat'),
    gulpif = require('gulp-if'),
    merge = require('merge-stream'),
    config = require('../config.js');

var postcssOptions = config.useCssMin ? [
    require('autoprefixer')({
        browsers: ['last 2 version', 'ie 10', 'ie 11']
    }),
    require('cssnext')(),
    require('precss')(),
    require('cssnano')()
] : null;

gulp.task('bundle-style', function () {
    var sass = config.useSass ? require('gulp-sass') : empty,
        postcss = config.useCssMin ? require('gulp-postcss') : empty;

    var appstream = gulp
        .src(path.join(config.src, '**/*.?(css|scss)'), {
            ignore: path.join(config.src, config.pagesDir, '**/*.?(css|scss)')
        })
        .pipe(gulpif(config.useSourcemaps, sourcemaps.init()))
        .pipe(gulpif(config.useSass, gulpif('*.scss', sass().on('error', sass.logError))))
        .pipe(gulpif(config.useCssMin, gulpif('*.css', postcss(postcssOptions))))
        .pipe(gulpif(config.useSourcemaps, sourcemaps.write()))
        .pipe(concat(config.destName + '.css'))
        .pipe(gulp.dest(config.dest));

    var pagesstream = gulp
        .src(path.join(config.src, config.pagesDir, '**/*.?(css|scss)'))
        .pipe(gulpif(config.useSourcemaps, sourcemaps.init()))
        .pipe(gulpif(config.useSass, gulpif('*.scss', sass().on('error', sass.logError))))
        .pipe(gulpif(config.useCssMin, gulpif('*.css', postcss(postcssOptions))))
        .pipe(gulpif(config.useSourcemaps, sourcemaps.write()))
        .pipe(gulp.dest(path.join(config.dest, config.pagesDir)));
    return merge(appstream, pagesstream);
});

// empty pipeline item
function empty() {
    var through = require('through2');
    return through.obj(function (file, enc, cb) {
        cb(null, file);
    });
}
