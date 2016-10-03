/*
 * Watch for .scss|.css|.js files update.
 *
 * 09/30/2016 [Ivan Kozhin] Initial version.
 */

var gulp = require('gulp'),
    path = require('path'),
    config = require('../config.js');

gulp.task('watch', function () {
    gulp.watch([path.join(config.src, '**/*.js')], [
        config.useRequireJS ? 'bundle-app-requirejs' : 'bundle-app',
        'bundle-app-pages']);
    gulp.watch([path.join(config.src, '**/*.scss')], ['bundle-style']);
});
