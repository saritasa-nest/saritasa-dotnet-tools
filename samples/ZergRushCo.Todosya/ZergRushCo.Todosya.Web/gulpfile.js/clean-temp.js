/*
 * Remove config.temp directory.
 *
 * 09/30/2016 [Ivan Kozhin] Initial version.
 * 12/14/2016 [Ivan Kozhin] Formatting.
 */

var gulp = require('gulp'),
    path = require('path'),
    runSequence = require('run-sequence'),
    del = require('del'),
    config = require('../config.js');

gulp.task('clean-temp', function (cb) {
    let fs = require('fs');
    if (!fs.existsSync(config.tempDir)) {
        del([path.join(config.tempDir, '**')], cb);
    }
    else {
        cb();
    }
});
