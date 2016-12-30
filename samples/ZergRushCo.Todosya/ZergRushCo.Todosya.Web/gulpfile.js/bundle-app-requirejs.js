/*
 * Bundle all .js files using r.js . Can be used with minify and babel.
 *
 * 09/30/2016 [Ivan Kozhin] Initial version.
 * 12/14/2016 [Ivan Kozhin] Formatting.
 */

var gulp = require('gulp'),
    path = require('path'),
    sourcemaps = require('gulp-sourcemaps'),
    concat = require('gulp-concat'),
    gulpif = require('gulp-if'),
    uglify = require('gulp-uglify'),
    replace = require('gulp-replace'),
    config = require('../config.js');

gulp.task('bundle-app-requirejs', function () {
    // RequireJS config
    var ConfigFile = require('requirejs-config-file').ConfigFile;
    var requirejsConfigFile = path.join(config.src, 'config.js');
    var configFile = new ConfigFile(requirejsConfigFile).read();

    // if we use ES2015 we have to compile to temp folder first
    var src = config.src;
    if (config.useBabel) {
        var babel = config.useBabel ? require('gulp-babel') : empty;
        gulp.src('./**/*.js', {
                cwd: config.src,
                ignore: path.join(config.pagesDir, '**/*.js')
            })
            .pipe(gulpif(config.useBabel, babel({ presets: ['es2015'] })))
            .pipe(gulp.dest(config.tempDir));
        src = config.tempDir;
    }

    // get require js dependencies
    var getRequirejsDeps = () => Object.keys(configFile.paths).map((key) => key);

    // additional js load
    var getAppDeps = function () {
        return require('glob').sync('**/*.js', {
            cwd: src,
            ignore: path.join(config.pagesDir, '**/*.js')
        }).map((filename) => filename.replace(/.js$/, ''))
    }

    // sum deps
    var includeDeps = [path.join('..', config.bower.src, 'requirejs/require')]
        .concat(getRequirejsDeps().concat(getAppDeps()))
    if (config.debug) {
        console.info('Bundled dependencies:');
        console.log(includeDeps);
    }

    // write null.js
    var fs = require('fs');
    fs.writeFileSync(path.join(config.dest, 'null.js'), '/* The file is to specify RequireJS baseUrl */');

    // js
    var requirejsOptimize = require('gulp-requirejs-optimize');
    var stream = gulp
        .src(config.entries.map((entry) => path.join(src, entry)))
        .pipe(gulpif(config.useSourcemaps, sourcemaps.init()))
        .pipe(requirejsOptimize({
            mainConfigFile: requirejsConfigFile,
            baseUrl: src,
            preserveLicenseComments: false,
            optimize: config.production ? 'uglify2' : 'none',
            findNestedDependencies: true,
            include: includeDeps,
            wrapShim: true,
            out: config.destName + '.js'
        }))
        .pipe(gulpif(config.useSourcemaps, sourcemaps.write()))
        .pipe(gulp.dest(config.dest));
    return stream;
});
