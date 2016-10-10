/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp');
var sass = require('gulp-sass');

gulp.task('sass', function () {
    gulp.src('./bower_components/**/*.scss')
    .pipe(sass())
    .pipe(gulp.dest('./bower_components'))
})

gulp.task('default', function () {
    // place code for your default task here
});