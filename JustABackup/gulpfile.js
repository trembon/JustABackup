"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    sass = require('gulp-sass'),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    sourcemaps = require('gulp-sourcemaps');

var paths = {
    webroot: "./wwwroot/",
    libs: "./node_modules/"
};
paths.concatJsDest = paths.webroot + "js/site.min.js";
paths.concatJsLibsDest = paths.webroot + "js/libs.min.js";
paths.concatCssDest = paths.webroot + "css/site.min.css";
paths.concatCssLibsDest = paths.webroot + "css/libs.min.css";


paths.jsLibs = [
    paths.libs + "jquery-cron/dist/jquery-cron.js",
    paths.libs + "jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.js"
];
paths.cssLibs = [
    paths.libs + "jquery-cron/dist/jquery-cron.css"
];


gulp.task("clean:js", function (cb) {
    rimraf(paths.webroot + "js/", cb);
});

gulp.task("clean:css", function (cb) {
    rimraf(paths.webroot + "css/", cb);
});

gulp.task("clean:fonts", function (cb) {
    rimraf(paths.webroot + "fonts/", cb);
});

gulp.task("clean", ["clean:js", "clean:css", "clean:fonts"]);


gulp.task('copy:fonts', function () {
    return gulp.src([paths.libs + 'font-awesome/fonts/*.*'])
        .pipe(gulp.dest(paths.webroot + 'fonts/'));
});


//gulp.task("compile:scss", function () {
//    return gulp.src(paths.scss)
//        .pipe(sourcemaps.init())
//        .pipe(sass().on("error", sass.logError))
//        .pipe(sourcemaps.write())
//        .pipe(gulp.dest(paths.css));
//});

//gulp.task('watch:scss', function () {
//    gulp.watch("./Styles/*.scss", ['compile:scss']);
//});


gulp.task("min:js", function () {
    return gulp.src([paths.webroot + "js/*.js", "!" + paths.concatJsDest, "!" + paths.concatJsLibsDest])
        .pipe(concat(paths.concatJsDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task("min:jslibs", function () {
    return gulp.src(paths.jsLibs)
        .pipe(concat(paths.concatJsLibsDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

//gulp.task("min:css", function () {
//    return gulp.src([paths.css + "css/*.css", "!" + paths.concatCssDest, "!" + paths.concatCssLibsDest])
//        .pipe(concat(paths.concatCssDest))
//        .pipe(cssmin())
//        .pipe(gulp.dest("."));
//});

gulp.task("min:csslibs", function () {
    return gulp.src(paths.cssLibs)
        .pipe(concat(paths.concatCssLibsDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("min", ["min:js", "min:jslibs", /*"min:css",*/ "min:csslibs"]);