using System;
using Android.App;
using Android.OS;
using Gauge;
using System.Timers;

namespace Demo
{
  [Activity( Label = "Demo", MainLauncher = true, Icon = "@drawable/icon" )]
  public class MainActivity : Activity
  {
    private Timer _timer;
    int value = 0;

    private GaugeView mGaugeView1;
    private GaugeView mGaugeView2; 
    private Random RAND = new Random();   

  protected override void OnCreate( Bundle bundle )
    {
      base.OnCreate( bundle );

      // Set our view from the "main" layout resource
      SetContentView( Resource.Layout.Main );
      mGaugeView1 = FindViewById<GaugeView>( Resource.Id.gauge_view1 );
      mGaugeView2 = FindViewById<GaugeView>( Resource.Id.gauge_view2 );
      mGaugeView1.SetTargetValue( value );
      mGaugeView2.SetTargetValue( value );

      _timer = new Timer()
      {
        Interval = 1000
      };
      //Trigger event every second      
      _timer.Elapsed += OnTimedEvent;
      
      _timer.Enabled = true;
    }

    private void OnTimedEvent( object sender, ElapsedEventArgs e )
    {
      RunOnUiThread(()=>
      {
        value = RAND.Next( 101 );
        mGaugeView1.SetTargetValue( value );
        mGaugeView2.SetTargetValue( value );
      } );      
    }
  }
}

