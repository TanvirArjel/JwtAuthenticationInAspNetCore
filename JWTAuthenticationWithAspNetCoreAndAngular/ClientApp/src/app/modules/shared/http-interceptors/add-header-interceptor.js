"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var AddHeaderInterceptor = /** @class */ (function () {
    function AddHeaderInterceptor(authenticationService) {
        this.authenticationService = authenticationService;
    }
    AddHeaderInterceptor.prototype.intercept = function (request, next) {
        var accessToken = this.authenticationService.getAccessToken();
        // Clone the request to add the new header
        var clonedRequest = request.clone({ headers: request.headers.set('Authorization', "Bearer " + accessToken) });
        // Pass the cloned request instead of the original request to the next handle
        return next.handle(clonedRequest);
    };
    AddHeaderInterceptor = __decorate([
        core_1.Injectable()
    ], AddHeaderInterceptor);
    return AddHeaderInterceptor;
}());
exports.AddHeaderInterceptor = AddHeaderInterceptor;
//# sourceMappingURL=add-header-interceptor.js.map