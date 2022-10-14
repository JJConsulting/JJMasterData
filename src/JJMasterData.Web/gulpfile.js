/// <binding AfterBuild='default' Clean='clean' />
/*
This file is the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/
const gulp = require("gulp");
const del = require("del");
const cleanCss = require("gulp-clean-css");
const terser = require("gulp-terser");
const concat = require("gulp-concat");

function cleanJJMasterDataScripts() {
    return del(["wwwroot/js/jjmasterdata/**/*"]);
}

function cleanCssBundles() {
    return del(["wwwroot/css/jjmasterdata/*-bundle-*.min.css"]);
}

function pipeJJMasterDataScripts() {
    const paths = {
        scripts: ["Scripts/**/*.js", "Scripts/**/*.js", "Scripts/**/*.map"],
    };

    gulp.src(paths.scripts).pipe(gulp.dest("wwwroot/js/jjmasterdata"));
}

function getCommonCss() {
    return [
        "wwwroot/css/flatpickr/*.css",
        "wwwroot/css/fontawesome/*.css",
        "wwwroot/css/highlightjs/ssms.min.css",
        "wwwroot/css/jjmasterdata/jjmasterdata.css",
        "wwwroot/css/bootstrap/bootstrap-tagsinput/bootstrap-tagsinput.css"
    ]
}

function bundleCss(files, bundleName) {
    gulp.src(files)
        .pipe(concat(`jjmasterdata-bundle-${bundleName}.min.css`))
        .pipe(cleanCss())
        .pipe(gulp.dest('wwwroot/css/jjmasterdata'))
}

function bundleCssBootstrap3() {
    const files = [
        "wwwroot/css/bootstrap/bootstrap3/*.css",
        "wwwroot/css/bootstrap/bootstrap-select/bootstrap-select.css",
        "wwwroot/css/bootstrap/bootstrap-toggle/bootstrap-toggle.min.css",
    ].concat(getCommonCss())

    bundleCss(files, "bootstrap3")
}

function bundleCssBootstrap4() {
    const files = [
        "wwwroot/css/bootstrap/bootstrap4/*.css",
        "wwwroot/css/bootstrap/bootstrap-select/bootstrap-select.css",
        "wwwroot/css/bootstrap/bootstrap-toggle/bootstrap-toggle.min.css",
    ].concat(getCommonCss())

    bundleCss(files, "bootstrap4")
}

function bundleCssBootstrap5() {
    const files = [
        "wwwroot/css/bootstrap/bootstrap5/*.css",
        "!wwwroot/css/bootstrap/bootstrap5/theme/**/*.css",
        "wwwroot/css/bootstrap/bootstrap-select/bootstrap-select-bs5.min.css",
        "wwwroot/css/bootstrap/bootstrap-toggle/bootstrap4-toggle.min.css",
    ].concat(getCommonCss())

    bundleCss(files, "bootstrap5")
}

function bundleCssBootstrap5WithTheme(theme) {
    const files = [
        `wwwroot/css/bootstrap/bootstrap5/theme/${theme}/*.css`,
        "wwwroot/css/bootstrap/bootstrap-select/bootstrap-select-bs5.min.css",
        "wwwroot/css/bootstrap/bootstrap-toggle/bootstrap4-toggle.min.css",
    ].concat(getCommonCss())

    bundleCss(files, "bootstrap5-" + theme)
}


function bundleCssWithoutBootstrap() {
    bundleCss(getCommonCss(), "without-bootstrap")
}

function getCommonJs() {
    return [
        "!wwwroot/js/bootstrap/**/*.js",
        "wwwroot/js/**/*.js",
        "wwwroot/js/bootstrap/bootstrap-tagsinput/bootstrap-tagsinput.min.js",
        "wwwroot/js/bootstrap/bootstrap-typeahead/bootstrap-typeahead.min.js",
    ]
}

function bundleJs(files, bundleName) {
    gulp.src(files)
        .pipe(concat(`jjmasterdata-bundle-${bundleName}.min.js`))
        .pipe(terser())
        .pipe(gulp.dest('wwwroot/js/jjmasterdata'))
}

function bundleJsWithoutBootstrap() {
    bundleJs(getCommonJs(), "without-bootstrap")
}

function bundleJsBootstrap3() {
    const files = [
        "wwwroot/js/bootstrap/bootstrap3/*.js",
        "wwwroot/js/bootstrap/bootstrap-select/bootstrap-select.min.js",
        "wwwroot/js/bootstrap/bootstrap3-toggle/bootstrap-toggle.min.js"
    ].concat(getCommonCss())

    bundleCss(files, "bootstrap3")
}

function bundleJsBootstrap4() {
    const files = [
        "wwwroot/js/bootstrap/bootstrap4/*.js",
        "wwwroot/js/bootstrap/bootstrap-select/bootstrap-select.min.js",
        "wwwroot/js/bootstrap/bootstrap4-toggle/bootstrap4-toggle.min.js"
    ].concat(getCommonCss())

    bundleCss(files, "bootstrap4")
}

function bundleJsBootstrap5() {
    const files = [
        "wwwroot/js/bootstrap/bootstrap4/*.js",
        "wwwroot/js/bootstrap/bootstrap-select/bootstrap-select-bs5.min.js",
        "wwwroot/js/bootstrap/bootstrap4-toggle/bootstrap4-toggle.min.js"
    ].concat(getCommonCss())

    bundleCss(files, "bootstrap5")
}

gulp.task("clean", function (done) {
    cleanJJMasterDataScripts();
    cleanCssBundles();
    done();
});

gulp.task("bundle-css", function (done) {
    bundleCssWithoutBootstrap();
    bundleCssBootstrap3();
    bundleCssBootstrap4();
    bundleCssBootstrap5();
    bundleCssBootstrap5WithTheme("jjmasterdata");
    bundleCssBootstrap5WithTheme("dark-blue");

    done();
});

gulp.task("bundle-js", function (done) {
    pipeJJMasterDataScripts();

    bundleJsWithoutBootstrap();
    bundleJsBootstrap3();
    bundleJsBootstrap4();
    bundleJsBootstrap5();

    done();
});
