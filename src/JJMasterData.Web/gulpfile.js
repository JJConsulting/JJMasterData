/// <binding AfterBuild='default' Clean='clean' />
/*
This file is the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/
const gulp = require("gulp");
const del = require("del");
const paths = {
    scripts: ["Scripts/**/*.js", "Scripts/**/*.js", "Scripts/**/*.map"],
};
gulp.task("clean", function () {
    return del(["wwwroot/js/jjmasterdata/**/*"]);
});
gulp.task("default", function (done) {
    gulp.src(paths.scripts).pipe(gulp.dest("wwwroot/js/jjmasterdata"));
    done();
});