var gulp = require('gulp'),
    path = require('path'),
    del = require('del'),
    rename = require('gulp-rename'),
    replace = require('gulp-replace'),
    merge = require('merge-stream'),
    config = require('../config.js');

gulp.task('custom', function () {
    var fontawesome = gulp.src([
            path.join(config.bower.src, 'font-awesome/fonts/**'),
            path.join(config.bower.src, 'bootstrap-sass/assets/fonts/bootstrap/**')
        ])
        .pipe(gulp.dest(path.join(config.dest, 'fonts')));

    var bootstrapfonts = gulp.src([path.join(config.dest, 'bundle.css'), path.join(config.dest, 'bundle.vendor.css')])
        .pipe(replace('../fonts/bootstrap/', './fonts/'))
        .pipe(replace('../fonts/fontawesome', './fonts/fontawesome'))
        .pipe(gulp.dest(config.dest));

    return merge(fontawesome, bootstrapfonts);
});
