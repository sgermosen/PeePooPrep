namespace PeePooFinder;

public partial class App : Application
{
	private readonly IServiceProvider _services;

	public App(IServiceProvider services)
	{
		InitializeComponent();
		_services = services;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var shell = _services.GetRequiredService<AppShell>();
		return new Window(shell);
	}
}
