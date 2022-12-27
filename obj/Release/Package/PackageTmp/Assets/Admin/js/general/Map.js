var latitude;
var longitude;
var PlaceId;
var IsMapLoaded = false;
var MapAddress;
var IsGetCurrentLocation = false;
var DefaultMapMarker = "/Assets/images/icons/marker-purple.png";

var lang = 'en';

function SetCurrentLocation(lat, lon, adr) {
    var currentLocation = {
        latitude: lat,
        longitude: lon,
        address: adr,
    }
    localStorage.setItem("currentLocation", JSON.stringify(currentLocation));
}

function getLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(showPosition);
        IsGetCurrentLocation = true;
    } else {
        alert("Geolocation is not supported by this browser.");

        latitude = 25.2048
        longitude = 55.2708
        InsertPosition(latitude, longitude);
        InsertLatLonInInput(latitude, longitude);
    }
}

function showPosition(position) {

    latitude = position.coords.latitude;
    longitude = position.coords.longitude;

    InsertLatLonInInput(latitude, longitude);
    InsertPosition(latitude, longitude);

}

function InsertPosition(latitude, longitude) {
    $.ajax({
        type: 'Get',
        url: `https://maps.googleapis.com/maps/api/geocode/json?latlng=${latitude},${longitude}&key=AIzaSyAEmhLaFjth5xau57Gy1NwE1O6apk443xY`,
        success: function (response) {
            if (response.results.length > 0) {
                
                MapAddress = response.results[0].formatted_address;

                InsertMapAddressInInput(MapAddress);

                if (IsGetCurrentLocation) {
                    SetCurrentLocation(latitude, longitude, MapAddress);
                    IsGetCurrentLocation = false;
                }

            } else {
                alert("Geolocation is not supported by this browser.");
            }
        }
    });
}

function InsertLatLonInInput(latitude, longitude, PlaceId) {
    $('#Latitude').val(latitude);
    $('#Longitude').val(longitude);
    $('#PlaceId').val(PlaceId);
}

function InsertMapAddressInInput(MapAddress) {
    $('#Address').val(MapAddress);
    $('#current-location').val(MapAddress);
}


function myMap() {


    //#region Old Getting Latitude and Longitude Function

    //var Latlng;
    //const myLatLng = { lat: 25.2048, lng: 55.2708 };
    //const map = new google.maps.Map(document.getElementById("googleMap"), {
    //	zoom: 13,
    //	center: myLatLng,
    //});
    //// Create the initial InfoWindow.
    //let infoWindow = new google.maps.InfoWindow({
    //	content: "Select Location",
    //	position: myLatLng,
    //});
    //new google.maps.Marker({
    //	position: myLatLng,
    //	map,
    //	title: "Hello World!",
    //});
    //infoWindow.open(map);
    //// Configure the click listener.
    //map.addListener("click", (mapsMouseEvent) => {
    //	// Close the current InfoWindow.
    //	infoWindow.close();
    //	// Create a new InfoWindow.
    //	infoWindow = new google.maps.InfoWindow({
    //		position: mapsMouseEvent.latLng,
    //	});
    //	infoWindow.setContent(
    //		JSON.stringify(mapsMouseEvent.latLng.toJSON(), null, 2),
    //		Latlng = mapsMouseEvent.latLng,
    //	);
    //	infoWindow.open(map);
    //	$('#Latitude').val(Latlng.lat);
    //	$('#Longitude').val(Latlng.lng);
    //});

    //#endregion
    //
    //if (typeof(element) == 'undefined')
    //{
    //    element = 'googleMap';
    //}

    if (!latitude) { latitude = 25.2048 }
    if (!longitude) { longitude = 55.2708 }

    var map = new google.maps.Map(document.getElementById("googleMap"), {
        zoom: 15,
        center: new google.maps.LatLng(latitude, longitude),
        mapTypeId: google.maps.MapTypeId.ROADMAP
    });

    //var mImage = new google.maps.MarkerImage(DefaultMapMarker,
    //	new google.maps.Size(34, 35),
    //	new google.maps.Point(0, 10),
    //	new google.maps.Point(10, 34)
    //);

    var myMarker = new google.maps.Marker({
        position: new google.maps.LatLng(latitude, longitude),
        draggable: true,
        icon: DefaultMapMarker,
    });

    google.maps.event.addListener(myMarker, 'dragend', function (evt) {
        document.getElementById('drag-map').innerHTML = '<span>' + ChangeString('Drag marker on the map to select your desired location.', 'اسحب علامة على الخريطة لتحديد الموقع المطلوب.') + '</span>';

        latitude = evt.latLng.lat().toFixed(3);
        longitude = evt.latLng.lng().toFixed(3);

        InsertLatLonInInput(latitude, longitude);
        InsertPosition(latitude, longitude);

    });
    google.maps.event.addListener(myMarker, 'dragstart', function (evt) {
        document.getElementById('drag-map').innerHTML = '<span>' + ChangeString('Currently dragging marker ...', 'جارٍ سحب العلامة حاليًا ...') + '</span>';
    });
    map.setCenter(myMarker.position);
    myMarker.setMap(map);

    //#region getLocationMap()

    //function getLocationMap() {
    //	if (navigator.geolocation) {
    //		navigator.geolocation.getCurrentPosition(showPosition);
    //	} else {
    //		x.innerHTML = "Geolocation is not supported by this browser.";
    //	}
    //}

    //function showPosition(position) {
    //	document.getElementById('drag-map').innerHTML = '<span>Drag marker on the map to select your desired location.</span>';

    //	latitude = position.coords.latitude;
    //	longitude = position.coords.longitude;

    //	InsertLatLonInInput(latitude, longitude);
    //	InsertPosition(latitude, longitude);

    //	var myMarker = new google.maps.Marker({
    //		position: new google.maps.LatLng(latitude, longitude),
    //		draggable: true,
    //		icon:  DefaultMapMarker
    //	});
    //	google.maps.event.addListener(myMarker, 'dragend', function (evt) {
    //		document.getElementById('drag-map').innerHTML = '<span>Drag marker on the map to select your desired location.</span>';
    //		latitude = position.coords.latitude;
    //		longitude = position.coords.longitude;

    //		InsertLatLonInInput(latitude, longitude);
    //		InsertPosition(latitude, longitude);
    //	});
    //	google.maps.event.addListener(myMarker, 'dragstart', function (evt) {
    //		document.getElementById('drag-map').innerHTML = '<span>' + ChangeString('Currently dragging marker ...', 'جارٍ سحب العلامة حاليًا ...') + '</span>';
    //	});
    //	map.setCenter(myMarker.position);
    //	myMarker.setMap(map);
    //}

    //getLocationMap();

    //#endregion
}

function initAutocomplete() {
   
    var defaultBounds = new google.maps.LatLngBounds();
    var options = {
        //types: [''],
        bounds: defaultBounds
    };

    var inputs = document.getElementsByClassName('Address');

    var autocompletes = [];

    for (var i = 0; i < inputs.length; i++) {
        var autocomplete = new google.maps.places.Autocomplete(inputs[i], options);
        autocomplete.inputId = inputs[i].id;
        autocomplete.addListener('place_changed', fillIn);
        autocompletes.push(autocomplete);
    }

    function fillIn() {

        var place = this.getPlace();


        latitude = place.geometry.location.lat();
        longitude = place.geometry.location.lng();
        PlaceId = place.place_id
        InsertLatLonInInput(latitude, longitude,PlaceId);

        myMap();

        MapAddress = place.formatted_address;

        InsertMapAddressInInput(MapAddress);

    }

    //#region Single Id Bound

    //var defaultBounds = new google.maps.LatLngBounds();
    //var options = {
    //	types: ['(cities)'],
    //	bounds: defaultBounds
    //};

    //// get DOM's input element
    //var input = document.getElementById('Address');

    //// Make Autocomplete instance
    //var autocomplete = new google.maps.places.Autocomplete(input, options);

    //// Listener for whenever input value changes
    //autocomplete.addListener('place_changed', function () {

    //	// Get place info
    //	var place = autocomplete.getPlace();

    //	// Do whatever with the value!
    //	latitude = place.geometry.location.lat();
    //	longitude = place.geometry.location.lng();
    //});

    //#endregion

    myMap();
}

function openMap() {


    if (!IsMapLoaded) {

        if (!latitude) {

            getLocation();
        }
        setTimeout(function () {
            myMap();
            $('.map-div-spin').hide();
            $('.map-div').show();
        }, 1000)
        IsMapLoaded = true;
    }
    else {
        myMap();
        InsertMapAddressInInput(MapAddress)
    }
    $('#map-modal').modal('show');
}

//function openMapWithoutPopup(element) {

//    
    
//    if (!IsMapLoaded) {

//        if (!latitude) {

//            getLocation();
//        }
//        setTimeout(function () {
//            myMap(element);
//            $('.map-div-spin').hide();
//            $('.map-div').show();
//        }, 1000)
//        IsMapLoaded = true;
//    }
//    else {
//        myMap();
//        InsertMapAddressInInput(MapAddress)
//    }
    
//}

function ChangeString(en_text, ar_text) {
    return lang == 'en' ? en_text : ar_text;
};









