var map = new ol.Map({
    target: 'map',
    layers: [
        new ol.layer.Tile({
            source: new ol.source.OSM()
        })
    ],
    view: new ol.View({
        center: ol.proj.fromLonLat([-0.1276, 51.5074]),
        zoom: 12
    })
});

// Global variable to store the marker layer
window.markerLayer = null;

// Function to add a marker and remove existing ones
window.addMarker = (lat, lon) => {
    // Remove the existing marker layer if it exists
    if (window.markerLayer) {
        window.map.removeLayer(window.markerLayer);
    }

    // Create a new marker feature
    const marker = new ol.Feature({
        geometry: new ol.geom.Point(ol.proj.fromLonLat([lon, lat]))
    });

    const markerStyle = new ol.style.Style({
        image: new ol.style.Circle({
            radius: 10, // Radius of the circle in pixels
            fill: new ol.style.Fill({ color: 'rgba(255, 0, 0, 0.8)' }), // Fill color (red with 80% opacity)
            stroke: new ol.style.Stroke({ color: 'black', width: 2 }) // Optional stroke (black border)
        })
    });

    // Apply the style to the marker
    marker.setStyle(markerStyle);

    // Create a vector source with the marker
    const vectorSource = new ol.source.Vector({
        features: [marker]
    });

    // Create a vector layer with the source
    window.markerLayer = new ol.layer.Vector({
        source: vectorSource
    });

    // Add the vector layer to the map
    window.map.addLayer(window.markerLayer);
};
