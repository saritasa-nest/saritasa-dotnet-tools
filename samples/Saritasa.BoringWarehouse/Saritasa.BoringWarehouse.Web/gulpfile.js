/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp');
var sass = require('gulp-sass');

var packagesRoot = './bower_components/';

gulp.task('sass', function () {
    gulp.src([
        packagesRoot + 'datatables-buttons/css/buttons.bootstrap.scss',
        packagesRoot + 'datatables-select/css/select.bootstrap.scss',
    ])
        .pipe(sass())
        .pipe(gulp.dest('./Static/css'));
})

gulp.task('css', function () {
    gulp.src([
        packagesRoot + 'bootstrap/dist/css/bootstrap.css',
        packagesRoot + 'datatables/media/css/dataTables.bootstrap.css',
    ]).pipe(gulp.dest('./Static/css'));
});

gulp.task('js', function () {
    gulp.src([
        packagesRoot + 'jquery/dist/jquery.js',
        packagesRoot + 'jquery-tmpl/jquery.tmpl.js',
        packagesRoot + 'jquery-validation/dist/jquery.validate.js',
        packagesRoot + 'datatables/media/js/jquery.dataTables.js',
        packagesRoot + 'datatables/media/js/dataTables.bootstrap.js',
        packagesRoot + 'datatables-buttons/js/dataTables.buttons.js',
        packagesRoot + 'datatables-buttons/js/buttons.bootstrap.js',
        packagesRoot + 'datatables-select/js/dataTables.select.js',
        packagesRoot + 'bootbox.js/bootbox.js',
        packagesRoot + 'bootstrap/dist/js/bootstrap.js',
    ]).pipe(gulp.dest('./Static/js'))
});

gulp.task('default', ['sass', 'css', 'js']);