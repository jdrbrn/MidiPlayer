using System.ComponentModel;
using System.Reflection;

namespace MidiPlayer
{
    internal class OutputChannel
    {
        readonly MidiPlayer.OutputModule.IOutputModule _outputModule;
        readonly MidiPlayer.ParsedTrack _track;

        public OutputChannel(ChannelConfiguration config, ParsedTrack track)
        {
            _track = track;
            _outputModule = CreateOutput(config);
        }

        public void Output()
        {
            _outputModule.Output(_track);
        }

        private static MidiPlayer.OutputModule.IOutputModule CreateOutput(ChannelConfiguration config)
        {
            // Supported OutputModules
            Dictionary<string, Type> outputModules = new()
            {
                ["MidiFile"] = typeof(MidiPlayer.OutputModule.MidiFile),
                ["ConsoleBeep"] = typeof(MidiPlayer.OutputModule.ConsoleBeep),
                ["PassiveBuzzer"] = typeof(MidiPlayer.OutputModule.PassiveBuzzer)
            };

            // Check if config file is calling for a non-existant module
            if (!outputModules.ContainsKey(config.OutputModule))
            {
                throw new Exception("Invalid Output Module type " + config.OutputModule + " in config file");
            }

            // Get paramaters for the constructor of the module
            ParameterInfo[] constructorParameters = outputModules[config.OutputModule].GetConstructors()[0].GetParameters();
            // Create an array to hold the needed parameters
            object[] parameters = new object[constructorParameters.Length];
            // Check for and copy called for parameters
            for (int i = 0; i < constructorParameters.Length; i++)
            {
                // Not nullable since only called if there is a parameter
                string parameterName = constructorParameters[i].Name!;
                // Check if paramater is given in the config file
                if (!config.Args.ContainsKey(parameterName))
                    throw new Exception("Missing argument " + parameterName + " for Output Module " + config.OutputModule);

                // If the paramater should be a string, just pass through
                if (constructorParameters[i].ParameterType == typeof(string)) parameters[i] = config.Args[parameterName];
                // Else it needs converting
                else
                {
                    // Get the needed converter
                    var convertor = TypeDescriptor.GetConverter(constructorParameters[i].ParameterType);
                    object arg;
                    // Try converting
                    try
                    {
                        // Don't care about possible null since it would cause an error that's catched
                        arg = convertor.ConvertFromString(config.Args[parameterName])!;
                    }
                    catch
                    {
                        throw new Exception("Invalid argument " + config.Args[parameterName] + " for argument " + parameterName + "for Output Module " + config.OutputModule);
                    }
                    // Store converted argument
                    parameters[i] = arg;
                }
            }
            // Call the constructor with the paramaters and return the configured OutputModule
            return (MidiPlayer.OutputModule.IOutputModule)outputModules[config.OutputModule].GetConstructors()[0].Invoke(parameters);
        }
    }
}
