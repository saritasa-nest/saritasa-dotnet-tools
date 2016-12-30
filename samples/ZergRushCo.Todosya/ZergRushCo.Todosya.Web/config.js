/*
 * Sample Saritasa Gulp file. Expected default files structure:
 *
 * App/ (main frontend directory, all .js files will be bundled)
 * |-- App.js (entry point)
 * |-- config.js (requirejs config)
 * |-- utils.js
 * |
 * |-- Pages/ (the directory has special meaning - its content will not be included to bundle)
 * |   |-- Users/
 * |   |   |-- registration.js
 * |   |   |-- style.scss
 * |   |-- Products/
 * |       |-- states.js
 * |       |-- style.scss
 * |
 * |-- Services/ (however this one will be on the bundle)
 * |   |-- UsersService.js
 * |   |-- ProductsService.js
 * |
 * |-- Style/ (the styles will be bundled as well, scss will be converted to css)
 * |   |-- app.scss
 * |   |-- custom.css
 *
 * Default output files structure:
 *
 * Static/
 * |-- bundles/
 *     |-- bundle.js
 *     |-- bundle.css
 *     |-- Pages/
 *         |-- Users/
 *         |   |-- registration.js
 *         |   |-- style.css
 *         |-- Products/
 *             |-- states.js
 *             |-- style.css
 *
 * To use initial structure with following command:
 * gulp: npm i gulp del gulp-uglify gulp-rename gulp-replace gulp-if gulp-sourcemaps gulp-concat run-sequence --save-dev
 * grunt: npm i grunt load-grunt-configs --save-dev
 * webpack: npm i webpack webpack-dev-server babel-loader babel-core babel-preset-es2015 babel-plugin-transform-runtime file-loader html-webpack-plugin --save-dev
 */

module.exports = {
    // show debug information
    debug: true,

    // frontend application main path
    src: './Frontend',

    // out directory
    dest: './Static/bundles',

    // dest base file name
    destName: 'bundle',

    // application frontend entry points
    entries: ['App.js'],

    // the files will not be bundled
    pagesDir: 'Pages',

    // production mode: minify is enabled
    production: false,

    bower: {
        // Bower packages path
        src: './bower_components'
    },

    // temporary processing folder
    tempDir: './temp',

    // generate source maps
    useSourcemaps: false,

    // use requirejs bundler
    // gulp: npm i gulp-requirejs-optimize requirejs-config-file --save-dev
    useRequireJS: true,
    
    // use babel for js processing
    // gulp: npm i gulp-babel babel-preset-es2015 --save-dev
    useBabel: false,

    // preprocess .scss files
    // gulp: npm i gulp-sass --save-dev
    useSass: true,

    // minify .html files
    // gulp: npm i gulp-htmlmin --save-dev
    useHtmlMin: false,

    // gulp: npm i gulp-postcss cssnano autoprefixer cssnext precss --save-dev
    useCssMin: false,

    // gulp: npm i jshint gulp-jshint gulp-eslint eslint-plugin-react babel-eslint eslint-plugin-html --save-dev
    useJsValidate: true,

    // gulp: npm i gulp-csslint --save-dev
    useCssValidate: false,

    // gulp: npm i main-bower-files --save-dev
    // grunt:
    useBowerJs: true,

    // gulp: npm i main-bower-files --save-dev
    useBowerSassCss: true
};
