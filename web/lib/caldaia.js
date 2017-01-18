angular
	.module("caldaia", [])
	.controller("theController", ['$scope', '$http', '$filter', '$interval', '$window', function ($scope, $http, $filter, $interval, $window) {
		$scope.isLoading = false;
		$scope.showDetails = false;
		$scope.current = {};

		$scope.toggleShowDetails = function () {
			$scope.showDetails = !$scope.showDetails;
		}

		function getTypeForPompaRiscaldamento() {
			var toReturn = '';
			toReturn = $scope.current.inTermoAmbienteValue === 1 ? 'pompaAccesa' : 'pompaSpenta';
			toReturn = $scope.current.outOverrideTermoAmbienteValue === 1 ? 'pompaAccesaConOverride' : toReturn;
			return toReturn;
		}

		function setConnectionStyle() {
			var current = $scope.current;
			plumbConnections.pompaCamino.setType(current.outPompaCaminoValue === 1 ? 'pompaAccesa' : 'pompaSpenta');
			plumbConnections.pompaCamino.endpoints[0].setType(current.outPompaCaminoValue === 1 ? 'pompaAccesa' : 'pompaSpenta');
			plumbConnections.pompaCamino.endpoints[1].setType(current.outPompaCaminoValue === 1 ? 'pompaAccesa' : 'pompaSpenta');
			plumbConnections.pompaCaldaia.setType(current.outCaldaiaValue === 1 ? 'pompaAccesa' : 'pompaSpenta');
			plumbConnections.pompaCaldaia.endpoints[0].setType(current.outCaldaiaValue === 1 ? 'pompaAccesa' : 'pompaSpenta');
			plumbConnections.pompaCaldaia.endpoints[1].setType(current.outCaldaiaValue === 1 ? 'pompaAccesa' : 'pompaSpenta');
			plumbConnections.pompaPannelli.setType(current.rotexP1 !== 0 ? 'pompaAccesa' : 'pompaSpenta');
			plumbConnections.pompaPannelli.endpoints[0].setType(current.rotexP1 !== 0 ? 'pompaAccesa' : 'pompaSpenta');
			plumbConnections.pompaPannelli.endpoints[1].setType(current.rotexP1 !== 0 ? 'pompaAccesa' : 'pompaSpenta');
			plumbConnections.pompaRiscaldamento.setType(getTypeForPompaRiscaldamento());
			plumbConnections.pompaRiscaldamento.endpoints[0].setType(getTypeForPompaRiscaldamento());
			plumbConnections.pompaRiscaldamento.endpoints[1].setType(getTypeForPompaRiscaldamento());
		};

		$scope.gotoDashboard = function () {
			$window.location.href='/kibana4/app/kibana#/dashboard/Caldaia';
		}
	
		$scope.loadData = function () {
			$scope.isLoading = true;
			$http.get('CURRENT.json?v=' + new Date().getTime())
				.success(function (dati) {
					$scope.currentAsString = $filter('json')(dati);
					angular.copy(dati, $scope.current);
					$scope.isLoading = false;
					$scope.errore = null;
					setConnectionStyle();
				})
				.error(function (dati, status) {
					$scope.isLoading = false;
					$scope.errore = status;
				})
		};
		$scope.loadData();

		// Imposta il reload automatico ogni 60 secondi.
		var autoreload = $interval(function () {
			$scope.loadData();
		}, 60000);
		$scope.$on('$destroy', function () {
			$interval.cancel(autoreload);
			autoreload = undefined;
		});
	}]);
