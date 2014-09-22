angular.module('blogAdmin').controller('ProfileController', ["$rootScope", "$scope", "$filter", "dataService", function ($rootScope, $scope, $filter, dataService) {
    $scope.user = {};
    $scope.noAvatar = SiteVars.ApplicationRelativeWebRoot + "pics/noavatar.jpg";
    $scope.photo = $scope.noAvatar;
    $scope.UserVars = UserVars;

    $scope.load = function () {
        spinOn();
        dataService.getItems('/api/users/' + UserVars.Name)
        .success(function (data) {
            angular.copy(data, $scope.user);
            $scope.setPhoto();
            spinOff();
        })
        .error(function () {
            toastr.error($rootScope.lbl.errorLoadingUser);
            spinOff();
        });
    }

    $scope.save = function () {
        spinOn();
        dataService.updateItem("/api/users/saveprofile/item", $scope.user)
        .success(function (data) {
            toastr.success($rootScope.lbl.userUpdatedShort);
            $scope.load();
            spinOff();
        })
        .error(function () {
            toastr.error($rootScope.lbl.updateFailed);
            spinOff();
        });
    }

    $scope.removePicture = function () {
        $scope.user.Profile.PhotoUrl = "";
        $scope.save();
    }

    $scope.changePicture = function (files) {
        var fd = new FormData();
        fd.append("file", files[0]);

        dataService.uploadFile("/api/upload?action=profile", fd)
        .success(function (data) {
            $scope.user.Profile.PhotoUrl = data;
            $scope.save();
        })
        .error(function () { toastr.error($rootScope.lbl.failed); });
    }

    $scope.setPhoto = function () {
        if ($scope.user.Profile.PhotoUrl) {
            $scope.photo = SiteVars.RelativeWebRoot + "image.axd?picture=/avatars/" +
                $scope.user.Profile.PhotoUrl + "&" + new Date().getTime();
        }
        else {
            $scope.photo = $scope.noAvatar;
        }
    }

    $scope.load();
}]);