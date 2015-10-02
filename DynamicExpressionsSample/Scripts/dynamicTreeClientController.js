angular.module('dynamicTreeDemoApp')
    .factory('dynamicTree.api', ['$resource', function ($resource) {
        return $resource('api/customer', {}, {
            getConditions: { url: 'api/customer/conditions' }
        });
    }])

.controller('dynamicTreeClientCtrl', ['$scope', 'dynamicTree.api', 'virtoCommerce.coreModule.common.dynamicExpressionService',
    function ($scope, dynamicTreeApi, dynamicExpressionService) {

        $scope.refresh = function () {
            var expressionToServer = angular.copy($scope.dynamicExpression);
            if (expressionToServer) {
                expressionToServer.$promise = undefined;
                expressionToServer.$resolved = undefined;
                stripOffUiInformation(expressionToServer);
            }
            $scope.entities = dynamicTreeApi.query(expressionToServer);
        };

        // Dynamic ExpressionBlock
        function extendElementBlock(expressionBlock) {
            var retVal = dynamicExpressionService.expressions[expressionBlock.Id];
            if (!retVal) {
                retVal = { displayName: 'unknown element: ' + expressionBlock.Id };
            }

            _.extend(expressionBlock, retVal);

            if (!expressionBlock.Children) {
                expressionBlock.Children = [];
            }
            _.each(expressionBlock.excludingCategoryIds, function (id) {
                expressionBlock.Children.push({ id: 'ExcludingCategoryCondition', selectedCategoryId: id });
            });
            _.each(expressionBlock.excludingProductIds, function (id) {
                expressionBlock.Children.push({ id: 'ExcludingProductCondition', productId: id });
            });

            _.each(expressionBlock.Children, extendElementBlock);
            _.each(expressionBlock.AvailableChildren, extendElementBlock);
            return expressionBlock;
        };

        function stripOffUiInformation(expressionElement) {
            expressionElement.AvailableChildren = undefined;
            expressionElement.displayName = undefined;
            expressionElement.getValidationError = undefined;
            expressionElement.groupName = undefined;
            expressionElement.newChildLabel = undefined;
            expressionElement.templateURL = undefined;

            _.each(expressionElement.Children, stripOffUiInformation);
        };
        // on load
        $scope.refresh();

        dynamicTreeApi.getConditions({}, function (data) {
            _.each(data.Children, extendElementBlock);
            $scope.dynamicExpression = data;
        });
    }]);