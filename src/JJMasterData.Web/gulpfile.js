/// <binding AfterBuild='default' Clean='clean' />
/*
This file is the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/
const gulp = require("gulp");
const del = require("del");
const cleanCss = require("gulp-clean-css");
const jsmin = require("gulp-terser");
const concat = require("gulp-concat");

function cleanJJMasterDataScripts() {
    return del(["wwwroot/js/jjmasterdata/**/*"]);
}

function cleanCssBundles() {
    return del(["wwwroot/css/*-bundle-*.min.css"]);
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

gulp.task("clean", function (done) {
    cleanJJMasterDataScripts();
    cleanCssBundles();
    done();
});

gulp.task("default", function (done) {
    pipeJJMasterDataScripts();

    bundleCssWithoutBootstrap();
    bundleCssBootstrap3();
    bundleCssBootstrap4();
    bundleCssBootstrap5();
    bundleCssBootstrap5WithTheme("jjmasterdata");
    bundleCssBootstrap5WithTheme("dark-blue");

    done();
});