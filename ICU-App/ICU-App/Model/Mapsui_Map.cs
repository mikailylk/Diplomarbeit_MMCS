using Mapsui.UI.Maui;
using Mapsui.Utilities;
using Mapsui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU_App.Model;

/// <summary>
/// This class provides static methods to work with Mapsui maps.
/// </summary>
public class Mapsui_Map
{
    /// <summary>
    /// Zooms to rectangle defined by two lat/long points.
    /// </summary>
    /// <param name="longitude_bigger">Longitude of the top right point of the rectangle.</param>
    /// <param name="latitude_bigger">Latitude of the top right point of the rectangle.</param>
    /// <param name="longitude_smaller">Longitude of the bottom left point of the rectangle.</param>
    /// <param name="latitude_smaller">Latitude of the bottom left point of the rectangle.</param>
    /// <param name="mapView">The map view to apply the zoom on.</param>
    public static void ZoomToRectangle(double longitude_bigger, double latitude_bigger, 
        double longitude_smaller, double latitude_smaller, MapView mapView)
    {
        // +- 0.001 --> if difference of long/lat are small --> still zoom to a proper rectangle
        Position pos_bigger = new Position(latitude_bigger + 0.001,  longitude_bigger + 0.001);
        MPoint mp_pos_bigger = pos_bigger.ToMapsui();

        Position pos_smaller = new Position(latitude_smaller - 0.0001, longitude_smaller - 0.0001);
        MPoint mp_smaller = pos_smaller.ToMapsui();

        // x1 --> Bottom Left
        // x2 --> Top Right
        // y1 --> Bottom Left
        // y2 --> Top Right

        double x1 = mp_smaller.X;
        double y1 = mp_smaller.Y;

        double x2 = mp_pos_bigger.X;
        double y2 = mp_pos_bigger.Y;

        double x_temp;
        double y_temp;

        // Check if really longitude/latitude_bigger is bigger than longitude/latitude_smaller ....

        if (x1 > x2)
        {
            x_temp = x2;
            x2 = x1;
            x1 = x_temp;
        }
        if (y1 > y2)
        {
            y_temp = y2;
            y2 = y1;
            y1 = y_temp;
        }
        MRect rect = new MRect(x1, y1, x2, y2);

        // zoom to rectangle
        mapView.Navigator.NavigateTo(rect, ScaleMethod.Fit, 0);
    }

    /// <summary>
    /// Zooms to a given point defined by a lat/long.
    /// </summary>
    /// <param name="longitude">Longitude of the point to zoom to.</param>
    /// <param name="latitude">Latitude of the point to zoom to.</param>
    /// <param name="mapView">The map view to apply the zoom on.</param>
    public static void ZoomToPoint(double longitude, double latitude, MapView mapView)
    {
        // convert coordinates to point and zoom
        Position currentpos = new Position(latitude, longitude);
        mapView.MyLocationLayer.UpdateMyLocation(currentpos);

        MPoint currentpos_point = currentpos.ToMapsui();

        mapView.Navigator.NavigateTo(currentpos_point, mapView.Map.Resolutions[19]);
    }

    /// <summary>
    /// Adds OpenStreetMap layer to the map, sets the CRS to longitude/latitude and clears the widgets.
    /// </summary>
    /// <param name="mapView">The map view to apply the material on.</param>
    public static void SetupMapMaterial(MapView mapView)
    {
        mapView.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        mapView.Map.CRS = "EPSG:4326";  // use longitude Latitude
        mapView.Map.Widgets.Clear();    // clears widgets
        mapView.Map.RotationLock = true;
    }

    /// <summary>
    /// Draws a red pin at the given longitude and latitude on the MapView.
    /// </summary>
    /// <param name="mapView">The MapView to draw the pin on.</param>
    /// <param name="longitude">The longitude of the pin.</param>
    /// <param name="latitude">The latitude of the pin.</param>
    public static void DrawRedPin(MapView mapView, double longitude, double latitude)
    {
        // clear pin if available
        mapView.Pins.Clear();

        // add the new pin
        mapView.Pins.Add(new Pin()
        {
            Position = new Position(latitude, longitude),
            Type = PinType.Pin,
            Label = "Tracked Device",
            Scale = 0.7F
        });
    }

    /// <summary>
    /// Draws a red trace between the given longitude and latitude points on the MapView.
    /// </summary>
    /// <param name="mapView">The MapView to draw the trace on.</param>
    /// <param name="longitude">The longitude points for the trace.</param>
    /// <param name="latitude">The latitude points for the trace.</param>
    public static void DrawTrace(MapView mapView, List<double>longitude, List<double> latitude)
    {
        // clear any existing drawables
        mapView.Drawables.Clear();

        // create the new polyline drawable
        // with strokethickness of 4
        Polyline polyline = new Polyline()
        {
            StrokeWidth = 4,
            StrokeColor = Colors.Red,
            IsClickable = true
        };

        // add points to the polyline
        for (int i = 0; i < longitude.Count; i++)
        {
            polyline.Positions.Add(new Position(latitude[i], longitude[i]));
        }

        // add the trace to the map
        mapView.Drawables.Add(polyline);
    }
}
