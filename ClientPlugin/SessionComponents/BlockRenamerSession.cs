using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game.Components;
using VRage.Utils;

namespace ClientPlugin.SessionComponents
{
	
	// uses https://steamcommunity.com/sharedfiles/filedetails/?id=2077166496 with some edits, all credit to respective owners
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]

	public class BlockRenamerSession : MySessionComponentBase {
		private readonly Dictionary<IMyTerminalBlock, string> _tempStringRenames = new Dictionary<IMyTerminalBlock, string>();

		private List<IMyTerminalControl> _controlsListMaster;

		private bool _setupDone;
		
		private int _index = 1;

		public override void UpdateBeforeSimulation()
		{
			if (_setupDone) return;
			_setupDone = true;
			_controlsListMaster = CreateControlList();
			MyAPIGateway.TerminalControls.CustomControlGetter += AddControlsToBlocks;
			MyAPIGateway.Utilities.InvokeOnGameThread(() => { this.SetUpdateOrder(MyUpdateOrder.NoUpdate); });

		}

		private void AddControlsToBlocks(IMyTerminalBlock block, List<IMyTerminalControl> controls)
		{
			if (block == null)
				return;

			controls.AddRange(_controlsListMaster);

			_index = 1;
		}

		private List<IMyTerminalControl> CreateControlList() {

			var controlList = new List<IMyTerminalControl>();

			//Separator
			var separator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTerminalBlock>("Renamer_Separator");
			separator.Enabled = block => true;
			separator.SupportsMultipleBlocks = true;
			controlList.Add(separator);

			//Label
			var label = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyTerminalBlock>("Renamer_Label");
			label.Enabled = block => true;
			label.SupportsMultipleBlocks = true;
			label.Label = MyStringId.GetOrCompute("Block Renaming Controls");
			controlList.Add(label);

			//Textbox
			var textbox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyTerminalBlock>("Renamer_Textbox");
			textbox.Enabled = block => true;
			textbox.SupportsMultipleBlocks = true;
			textbox.Title = MyStringId.GetOrCompute("New Naming");
			textbox.Getter = block => {
				if (_tempStringRenames.TryGetValue(block, out var storedString) == false) {

					storedString = "";

				}

				return new StringBuilder(storedString);

			};
			textbox.Setter = (block, builder) => {
				if (_tempStringRenames.TryGetValue(block, out _) == false) {

					_tempStringRenames.Add(block, builder.ToString());

				} else {

					_tempStringRenames[block] = builder.ToString();

				}

			};
			controlList.Add(textbox);

			//Replace Name
			var replaceName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>("Renamer_RenameButton");
			replaceName.Enabled = block => true;
			replaceName.SupportsMultipleBlocks = true;
			replaceName.Title = MyStringId.GetOrCompute("Replace Name");
			replaceName.Action = block => {
				if (_tempStringRenames.TryGetValue(block, out var storedString) == false) {

					storedString = "";

				}

				block.CustomName = storedString;

			};
			controlList.Add(replaceName);

			//Prefix Name
			var prefixName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>("Renamer_PrefixButton");
			prefixName.Enabled = block => true;
			prefixName.SupportsMultipleBlocks = true;
			prefixName.Title = MyStringId.GetOrCompute("Prefix Name");
			prefixName.Action = block => {
				if (_tempStringRenames.TryGetValue(block, out var storedString) == false) {

					storedString = "";

				}

				storedString += block.CustomName;
				block.CustomName = storedString;

			};
			controlList.Add(prefixName);

			//Suffix Name
			var suffixName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>("Renamer_SuffixButton");
			suffixName.Enabled = block => true;
			suffixName.SupportsMultipleBlocks = true;
			suffixName.Title = MyStringId.GetOrCompute("Suffix Name");
			suffixName.Action = block => {
				if (_tempStringRenames.TryGetValue(block, out var storedString) == false) {

					storedString = "";

				}

				block.CustomName += storedString;

			};
			controlList.Add(suffixName);
			
			
			//cascade Name
			var cascadeName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>("Renamer_CascadeButton");
			cascadeName.Enabled = block => true;
			cascadeName.SupportsMultipleBlocks = true;
			cascadeName.Title = MyStringId.GetOrCompute("Cascade rename");
			cascadeName.Action = block => {

				block.CustomName = $"{block.DefinitionDisplayNameText} {_index}";
				_index++;
			};
			controlList.Add(cascadeName);
			
			//Reset Name
			var resetName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTerminalBlock>("Renamer_ResetButton");
			resetName.Enabled = block => true;
			resetName.SupportsMultipleBlocks = true;
			resetName.Title = MyStringId.GetOrCompute("Reset Name");
			resetName.Action = block => {

				block.CustomName = block.DefinitionDisplayNameText;

			};
			controlList.Add(resetName);

			return controlList;

		}
	}
}