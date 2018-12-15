(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["main"],{

/***/ "./src/$$_lazy_route_resource lazy recursive":
/*!**********************************************************!*\
  !*** ./src/$$_lazy_route_resource lazy namespace object ***!
  \**********************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

function webpackEmptyAsyncContext(req) {
	// Here Promise.resolve().then() is used instead of new Promise() to prevent
	// uncaught exception popping up in devtools
	return Promise.resolve().then(function() {
		var e = new Error('Cannot find module "' + req + '".');
		e.code = 'MODULE_NOT_FOUND';
		throw e;
	});
}
webpackEmptyAsyncContext.keys = function() { return []; };
webpackEmptyAsyncContext.resolve = webpackEmptyAsyncContext;
module.exports = webpackEmptyAsyncContext;
webpackEmptyAsyncContext.id = "./src/$$_lazy_route_resource lazy recursive";

/***/ }),

/***/ "./src/app/app.component.css":
/*!***********************************!*\
  !*** ./src/app/app.component.css ***!
  \***********************************/
/*! no static exports found */
/***/ (function(module, exports) {

module.exports = ".layout-topbar {\r\n  height: 55px;\r\n  background-color: black;\r\n  color: white;\r\n  box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.2), 0 1px 10px 0 rgba(0, 0, 0, 0.12), 0 4px 5px 0 rgba(0, 0, 0, 0.14);\r\n  box-sizing: border-box;\r\n}\r\n\r\n.main-content{\r\n  background-color: white;\r\n  margin-top: 2rem;\r\n}\r\n\r\n.device {\r\n  \r\n}\r\n\r\n.monitor {\r\n  display: inline-block;\r\n  font-family: 'Major Mono Display', monospace;\r\n  font-size: 3rem;\r\n  width: 100%;\r\n  text-align: center;\r\n  vertical-align: bottom;\r\n}"

/***/ }),

/***/ "./src/app/app.component.html":
/*!************************************!*\
  !*** ./src/app/app.component.html ***!
  \************************************/
/*! no static exports found */
/***/ (function(module, exports) {

module.exports = "<div class=\"fluid-container\">\n    <div class=\"row layout-topbar\">\n        <div class=\"col-md-12\">\n            <div class=\"col-md-3\">&nbsp;</div>\n        </div>\n    </div>\n    <div class=\"row\">\n        <p-toolbar class=\"col-md-12\">\n            <div class=\"ui-toolbar-group-left\">\n                <button pButton type=\"button\" class=\"ui-button-success\" icon=\"pi pi-reload\" label=\"Refresh Data\"\n                    (click)=\"refreshData()\"></button>\n                <button pButton type=\"button\" class=\"ui-button-success\" icon=\"pi pi-reload\" label=\"Refresh Runtime Settings\"\n                    (click)=\"refreshSettings()\"></button>\n            </div>\n        </p-toolbar>\n    </div>\n    <div class=\"row justify-content-center main-content\">\n        <div class=\"col-md-3 col-lg-3 device\">\n            <p-panel header=\"Pannelli Solari\">\n                <div class=\"monitor\">{{data.rotexTK}}&deg;C<i [ngClass]=\"{'pi-spin': data.rotexP1}\" [ngStyle]=\"{'color': data.rotexP1 ? 'green' : 'gray'}\"\n                        class=\"pi pi-cog\"></i></div>\n            </p-panel>\n        </div>\n        <div class=\"col-md-3 col-lg-3 device\">\n            <p-panel header=\"Accumulo Rotex\">\n                <div class=\"monitor\">{{data.rotexTS}}&deg;C</div>\n            </p-panel>\n        </div>\n        <div class=\"col-md-3 col-lg-3 device\">\n            <p-panel header=\"Stato Caldaia\">\n                <div class=\"monitor\"><i [ngClass]=\"{'pi-spin': data.outCaldaiaValue}\" [ngStyle]=\"{'color': data.outCaldaiaValue ? 'green' : 'gray'}\"\n                        class=\"pi pi-cog\"></i></div>\n            </p-panel>\n        </div>\n    </div>\n\n    <div class=\"row\">\n        <div class=\"col-md-2\">\n            <pre>{{data | json}}</pre>\n        </div>\n        <div class=\"col-md-2\">\n            <pre>{{settings | json}}</pre>\n        </div>\n    </div>\n\n</div>\n\n<p-growl [(value)]=\"msgs\"></p-growl>"

/***/ }),

/***/ "./src/app/app.component.ts":
/*!**********************************!*\
  !*** ./src/app/app.component.ts ***!
  \**********************************/
/*! exports provided: AppComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "AppComponent", function() { return AppComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _backend_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./backend.service */ "./src/app/backend.service.ts");
var __decorate = (undefined && undefined.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (undefined && undefined.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};


var AppComponent = /** @class */ (function () {
    function AppComponent(_backend) {
        this._backend = _backend;
        this.data = {};
        this.settings = {};
        this.msgs = [];
    }
    AppComponent.prototype.ngOnInit = function () {
        var _this = this;
        this._hubConnection = $.hubConnection('http://localhost:32767/signalr');
        this.dataProxy = this._hubConnection.createHubProxy('data');
        this.settingsProxy = this._hubConnection.createHubProxy('settings');
        this._hubConnection
            .start()
            .done(function () { return console.log('Connection started!'); })
            .catch(function (err) { return console.log('Error while establishing connection :('); });
        this.dataProxy.on('notify', function (payload) {
            console.log('SignalR: Received data on dataProxy', payload);
            _this.data = payload;
        });
        this.settingsProxy.on('notify', function (payload) {
            console.log('SignalR: Received data on settingsProxy', payload);
            _this.settings = payload;
        });
    };
    AppComponent.prototype.refreshData = function () {
        this._backend.updateLatestData().subscribe();
    };
    AppComponent.prototype.refreshSettings = function () {
        this._backend.updateLatestSettings().subscribe();
    };
    __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"])(),
        __metadata("design:type", Object)
    ], AppComponent.prototype, "data", void 0);
    __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"])(),
        __metadata("design:type", Object)
    ], AppComponent.prototype, "settings", void 0);
    __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"])(),
        __metadata("design:type", Array)
    ], AppComponent.prototype, "msgs", void 0);
    AppComponent = __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"])({
            selector: 'app-root',
            template: __webpack_require__(/*! ./app.component.html */ "./src/app/app.component.html"),
            styles: [__webpack_require__(/*! ./app.component.css */ "./src/app/app.component.css")],
            providers: [_backend_service__WEBPACK_IMPORTED_MODULE_1__["BackendService"]]
        }),
        __metadata("design:paramtypes", [_backend_service__WEBPACK_IMPORTED_MODULE_1__["BackendService"]])
    ], AppComponent);
    return AppComponent;
}());



/***/ }),

/***/ "./src/app/app.module.ts":
/*!*******************************!*\
  !*** ./src/app/app.module.ts ***!
  \*******************************/
/*! exports provided: AppModule */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "AppModule", function() { return AppModule; });
/* harmony import */ var _angular_platform_browser__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/platform-browser */ "./node_modules/@angular/platform-browser/fesm5/platform-browser.js");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _angular_platform_browser_animations__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/platform-browser/animations */ "./node_modules/@angular/platform-browser/fesm5/animations.js");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/forms */ "./node_modules/@angular/forms/fesm5/forms.js");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/common/http */ "./node_modules/@angular/common/fesm5/http.js");
/* harmony import */ var primeng_primeng__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! primeng/primeng */ "./node_modules/primeng/primeng.js");
/* harmony import */ var primeng_primeng__WEBPACK_IMPORTED_MODULE_5___default = /*#__PURE__*/__webpack_require__.n(primeng_primeng__WEBPACK_IMPORTED_MODULE_5__);
/* harmony import */ var primeng_toolbar__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! primeng/toolbar */ "./node_modules/primeng/toolbar.js");
/* harmony import */ var primeng_toolbar__WEBPACK_IMPORTED_MODULE_6___default = /*#__PURE__*/__webpack_require__.n(primeng_toolbar__WEBPACK_IMPORTED_MODULE_6__);
/* harmony import */ var _app_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./app.component */ "./src/app/app.component.ts");
/* harmony import */ var _backend_service__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./backend.service */ "./src/app/backend.service.ts");
var __decorate = (undefined && undefined.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};













var AppModule = /** @class */ (function () {
    function AppModule() {
    }
    AppModule = __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["NgModule"])({
            declarations: [
                _app_component__WEBPACK_IMPORTED_MODULE_7__["AppComponent"]
            ],
            imports: [
                _angular_platform_browser__WEBPACK_IMPORTED_MODULE_0__["BrowserModule"],
                _angular_platform_browser_animations__WEBPACK_IMPORTED_MODULE_2__["BrowserAnimationsModule"],
                _angular_forms__WEBPACK_IMPORTED_MODULE_3__["FormsModule"],
                _angular_common_http__WEBPACK_IMPORTED_MODULE_4__["HttpClientModule"],
                primeng_primeng__WEBPACK_IMPORTED_MODULE_5__["AccordionModule"],
                primeng_primeng__WEBPACK_IMPORTED_MODULE_5__["PanelModule"],
                primeng_primeng__WEBPACK_IMPORTED_MODULE_5__["ButtonModule"],
                primeng_primeng__WEBPACK_IMPORTED_MODULE_5__["RadioButtonModule"],
                primeng_primeng__WEBPACK_IMPORTED_MODULE_5__["GrowlModule"],
                primeng_toolbar__WEBPACK_IMPORTED_MODULE_6__["ToolbarModule"]
            ],
            providers: [_angular_common_http__WEBPACK_IMPORTED_MODULE_4__["HttpClient"], _backend_service__WEBPACK_IMPORTED_MODULE_8__["BackendService"]],
            bootstrap: [_app_component__WEBPACK_IMPORTED_MODULE_7__["AppComponent"]]
        })
    ], AppModule);
    return AppModule;
}());



/***/ }),

/***/ "./src/app/backend.service.ts":
/*!************************************!*\
  !*** ./src/app/backend.service.ts ***!
  \************************************/
/*! exports provided: BackendService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BackendService", function() { return BackendService; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/common/http */ "./node_modules/@angular/common/fesm5/http.js");
var __decorate = (undefined && undefined.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (undefined && undefined.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};


var httpOptions = {
    headers: new _angular_common_http__WEBPACK_IMPORTED_MODULE_1__["HttpHeaders"]({ 'Content-Type': 'application/json' })
};
var BackendService = /** @class */ (function () {
    function BackendService(http) {
        this.http = http;
    }
    BackendService.prototype.getLatestData = function () {
        return this.http.get('http://localhost:32767/api/queries/latestdata');
    };
    BackendService.prototype.updateLatestData = function () {
        return this.http.get('http://localhost:32767/api/commands/get');
    };
    BackendService.prototype.updateLatestSettings = function () {
        return this.http.get('http://localhost:32767/api/commands/reloadsettings');
    };
    BackendService = __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["Injectable"])(),
        __metadata("design:paramtypes", [_angular_common_http__WEBPACK_IMPORTED_MODULE_1__["HttpClient"]])
    ], BackendService);
    return BackendService;
}());



/***/ }),

/***/ "./src/environments/environment.ts":
/*!*****************************************!*\
  !*** ./src/environments/environment.ts ***!
  \*****************************************/
/*! exports provided: environment */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "environment", function() { return environment; });
// This file can be replaced during build by using the `fileReplacements` array.
// `ng build ---prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.
var environment = {
    production: false
};
/*
 * In development mode, to ignore zone related error stack frames such as
 * `zone.run`, `zoneDelegate.invokeTask` for easier debugging, you can
 * import the following file, but please comment it out in production mode
 * because it will have performance impact when throw error
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.


/***/ }),

/***/ "./src/main.ts":
/*!*********************!*\
  !*** ./src/main.ts ***!
  \*********************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _angular_platform_browser_dynamic__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/platform-browser-dynamic */ "./node_modules/@angular/platform-browser-dynamic/fesm5/platform-browser-dynamic.js");
/* harmony import */ var _app_app_module__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./app/app.module */ "./src/app/app.module.ts");
/* harmony import */ var _environments_environment__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./environments/environment */ "./src/environments/environment.ts");




if (_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].production) {
    Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["enableProdMode"])();
}
Object(_angular_platform_browser_dynamic__WEBPACK_IMPORTED_MODULE_1__["platformBrowserDynamic"])().bootstrapModule(_app_app_module__WEBPACK_IMPORTED_MODULE_2__["AppModule"])
    .catch(function (err) { return console.log(err); });


/***/ }),

/***/ 0:
/*!***************************!*\
  !*** multi ./src/main.ts ***!
  \***************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

module.exports = __webpack_require__(/*! D:\play\caldaia\frontend\caldaia-frontend\src\main.ts */"./src/main.ts");


/***/ })

},[[0,"runtime","vendor"]]]);
//# sourceMappingURL=main.js.map