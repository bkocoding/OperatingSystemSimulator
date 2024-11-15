global using System.Collections.Immutable;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using OperatingSystemSimulator.Models;
#if MAUI_EMBEDDING
global using OperatingSystemSimulator.MauiControls;
#endif
global using ApplicationExecutionState = Windows.ApplicationModel.Activation.ApplicationExecutionState;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using OperatingSystemSimulator.Pages;
global using OperatingSystemSimulator.Pages.BIOSSettings;
global using OperatingSystemSimulator.ViewModels.PageViewModels;
global using OperatingSystemSimulator.Hardware;
global using static OperatingSystemSimulator.EventHandlers.KeyboardEventsHandler;
