﻿(function () {
 //新建模块名为app的模块，依赖于tm.pagination模块
    angular.module('app').controller('app.views.student.index', [//, ['tm.pagination']
        '$scope', '$uibModal', 'abp.services.app.student',
        function ($scope, $uibModal, $studentService) {
            var vm = this;
            vm.students = [];
            //function getStudents(postData) {
            //    postData = postData || {};
            //    $studentService.getStudentsAsync(postData).then(function (result) {
            //        // console.log(result);
            //        vm.students = result.data.items;
            //    });
            //}

            vm.openStudentCreationModal = function (id) {
                $scope.id = id;
                var modalInstance = $uibModal.open({
                    templateUrl: '/App/Main/views/students/createModal.cshtml',
                    controller: 'app.views.student.createModal as vm',
                    backdrop: 'static',
                    //,resolve: {//这是一个入参,这个很重要,它可以把主控制器中的参数传到模态框控制器中
                    //    items: function () {//items是一个回调函数
                    //        return id;//这个值会被模态框的控制器获取到
                    //    }
                    //},
                    scope: $scope
                });
                //console.log(modalInstance);
                modalInstance.result.then(function () {
                    getStudents();
                });
            };
            vm.deleteStudent = function (student) {
                abp.message.confirm(
                    App.localize('StudentDeleteWarningMessage', student.cnName),
                    function (isConfirmed) {
                        if (isConfirmed) {
                            $studentService.deleteStudent(student.id)
                                .then(function () {
                                    abp.notify.info(App.localize('DeleteSuccessfully'));
                                    getStudents();
                                });
                        }
                    }
                )
            };

            vm.updateActiveState = function (student) {
                $studentService.updateActiveState(student)
                    .then(function () {
                        abp.notify.info(App.localize('ToggleSuccessfully'));
                        getStudents();
                    });
            };


            //分页
            function getStudents() {
                // 发送给后台的请求数据
                var postData = {
                    pIndex: $scope.paginationConf.currentPage,
                    pSize: $scope.paginationConf.itemsPerPage
                };
                $studentService.getPagedStudents(postData).then(function (result) {
                    console.log(result);
                    console.log(result.data.totalCount);
                    console.log(result.data.items);
                    // 变更分页的总数
                    $scope.paginationConf.totalItems = result.data.totalCount;
                    // 变更产品条目
                    $scope.students = result.data.items;
                    vm.students = result.data.items;

                    console.log($scope.paginationConf);
                });

            }

            //配置分页基本参数
            $scope.paginationConf = {
                currentPage: 1,
                itemsPerPage: 2
            };

            /***************************************************************
            当页码和页面记录数发生变化时监控后台查询
            如果把currentPage和itemsPerPage分开监控的话则会触发两次后台事件。
            通过$watch currentPage和itemperPage 当他们一变化的时候，重新获取数据条目
            ***************************************************************/
            $scope.$watch('paginationConf.currentPage + paginationConf.itemsPerPage', getStudents);

            //getStudents();
        }
    ]);
})();