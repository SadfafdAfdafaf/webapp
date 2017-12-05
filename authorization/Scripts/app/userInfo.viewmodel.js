function UserInfoViewModel(app, name, dataModel) {
    var self = this;

    // Данные
    self.name = ko.observable(name);

    // Операции
    self.logOff = function () {
        dataModel.logout().done(function () {
            app.navigateToLoggedOff();
        }).fail(function () {
            app.errors.push("Не удалось выполнить выход.");
        });
    };

    self.manage = function () {
        app.navigateToManage();
    };
}
