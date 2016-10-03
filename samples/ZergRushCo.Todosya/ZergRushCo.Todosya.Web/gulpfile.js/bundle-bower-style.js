/*
 * Concatenate bower .scss|.css frontend files.
 *
 * 09/30/2016 [Ivan Kozhin] Initial version.
 */

var gulp = require('gulp'),
    path = require('path'),
    concat = require('gulp-concat'),
    gulpif = require('gulp-if'),
    rename = require('gulp-rename'),
    config = require('../config.js');

var postcssOptions = config.useCssMin ? [
    require('autoprefixer')({
        browsers: ['last 2 version', 'ie 10', 'ie 11']
    }),
    require('cssnext')(),
    require('precss')(),
    require('cssnano')()
] : null;

gulp.task('bundle-bower-style', function () {
    var mainbowerfiles = require('main-bower-files'),
        postcss = config.useCssMin ? require('gulp-postcss') : empty,
        sass = config.useSass ? require('gulp-sass') : empty;
    let bowerfiles = mainbowerfiles({
        base: config.bowerDir,
        filter: '**/*.?(scss|css)'
    });
    if (config.debug) {
        console.info('Bower style dependencies:');
        console.log(bowerfiles);
    }

    return gulp.src(bowerfiles)
        .pipe(gulpif(config.useSass, gulpif('*.scss', sass().on('error', sass.logError))))
        .pipe(gulpif(config.useSass, gulpif('*.scss', rename((dir, base, ext) => base + '.css'))))
        .pipe(gulpif(config.useCssMin, postcss(postcssOptions)))
        .pipe(concat(config.destName + '.vendor.css'))
        .pipe(gulp.dest(config.dest));
});

// empty pipeline item
function empty () {
    var through = require('through2');
    return through.obj(function (file, enc, cb) {
        cb(null, file);
    });
}
