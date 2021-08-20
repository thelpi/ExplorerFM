using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ExplorerFM.Datas;
using static ExplorerFM.Converters.PlayerPositioningDisplayConverter;

namespace ExplorerFM
{
    /// <summary>
    /// Logique d'interaction pour ClubWindow.xaml
    /// </summary>
    public partial class ClubWindow : Window
    {
        private const string PlayerPositionTemplateKey = "PlayerPositionTemplate";
        private const string PlayerNameTemplateKey = "PlayerNameTemplate";
        private const string NoClub = "Without club";

        private readonly Club _club;
        private readonly List<Player> _players;
        private readonly DataProvider _dataProvider;
        private readonly Dictionary<Tuple<Position, Side>, List<PlayerRateItemData>> _playersRateByPosition;

        public ClubWindow(DataProvider dataProvider, Club club, List<Player> players)
        {
            InitializeComponent();

            _dataProvider = dataProvider;
            _club = club;
            _players = players;
            
            _playersRateByPosition = GetRatedPlayersForAll();

            Title = club?.LongName ?? NoClub;
            PlayersView.ItemsSource = _players;
            PositionsComboBox.ItemsSource = Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = Enum.GetValues(typeof(Side));
            TacticsComboBox.ItemsSource = Tactic.Tactics;
        }

        private void PlayersView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var pItem = PlayersView.SelectedItem;
            if (pItem != null)
            {
                Hide();
                new PlayerWindow(pItem as Player).ShowDialog();
                ShowDialog();
            }
        }

        private void PositionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetRatedPlayersListBox();
        }

        private void SidesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetRatedPlayersListBox();
        }

        private void SetRatedPlayersListBox()
        {
            if (PositionsComboBox.SelectedIndex >= 0
                && SidesComboBox.SelectedIndex >= 0)
            {
                RatedPlayersListBox.ItemsSource = GetRatedPlayers(
                    (Position)PositionsComboBox.SelectedItem,
                    (Side)SidesComboBox.SelectedItem);
            }
        }

        private void TacticsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TacticsComboBox.SelectedItem != null)
            {
                TacticPlayersGrid.Children.Clear();
                var cumul = 0;
                var tactic = TacticsComboBox.SelectedItem as Tactic;

                var playersRateByPositionFilteredOrdered = _playersRateByPosition
                    .Where(prp => tactic.Positions.Contains(prp.Key))
                    .OrderByDescending(prp => prp.Value.Max(p => p.PercentOfTotal))
                    .ToList();

                while (playersRateByPositionFilteredOrdered.Count > 0)
                {
                    var prp = playersRateByPositionFilteredOrdered[0];

                    var i = 0;
                    var posGroup = tactic.Positions.Where(tp => tp.Item1 == prp.Key.Item1 && tp.Item2 == prp.Key.Item2);
                    var countGroup = posGroup.Count();
                    var playersToRemove = new List<PlayerRateItemData>();
                    foreach (var pos in posGroup)
                    {
                        var bestPlayer = prp.Value.ElementAt(i);
                        playersToRemove.Add(bestPlayer);

                        var chip = this.GetByTemplateKey<Ellipse>(PlayerPositionTemplateKey);
                        var text = this.GetByTemplateKey<TextBlock>(PlayerNameTemplateKey);

                        var rate = (int)Math.Round((bestPlayer.AttributeRate / (decimal)1000) * 20);
                        cumul += rate;
                        var posRateItem = new PositioningItem(pos.Item1, pos.Item2, rate);

                        chip.Fill = new SolidColorBrush(posRateItem.Color);
                        text.Text = bestPlayer.Name;

                        int colIndex;
                        if (pos.Item2 == Side.Center)
                        {
                            if (countGroup == 3)
                                colIndex = 1 + i;
                            else if (countGroup == 2)
                                colIndex = i == 0 ? 1 : 3;
                            else
                                colIndex = 2;
                        }
                        else if (pos.Item2 == Side.Right)
                            colIndex = 4;
                        else
                            colIndex = 0;

                        var rowIndex = Array.IndexOf(Positions, pos.Item1);

                        chip.SetValue(Grid.ColumnProperty, colIndex);
                        chip.SetValue(Grid.RowProperty, rowIndex);
                        text.SetValue(Grid.ColumnProperty, colIndex);
                        text.SetValue(Grid.RowProperty, rowIndex);

                        TacticPlayersGrid.Children.Add(chip);
                        TacticPlayersGrid.Children.Add(text);

                        i++;
                    }
                    playersRateByPositionFilteredOrdered.RemoveAt(0);
                    playersRateByPositionFilteredOrdered
                        .ForEach(prpTmp => prpTmp.Value.RemoveAll(pp => playersToRemove.Any(pr => pr.Id == pp.Id)));
                    playersRateByPositionFilteredOrdered = playersRateByPositionFilteredOrdered
                        .OrderByDescending(prpTmp => prpTmp.Value.Max(p => p.PercentOfTotal))
                        .ToList();
                }

                TacticInfoLabel.Content = $"Total value: {cumul}";
            }
        }

        private List<PlayerRateItemData> GetRatedPlayers(Position position, Side side)
        {
            var playersRatedList = new List<PlayerRateItemData>();
            foreach (var p in _players)
            {
                var rate = p.GetPositionSideRate(position, side);
                playersRatedList.Add(new PlayerRateItemData
                {
                    AttributeRate = (int)Math.Round((p.AttributesTotal * rate) / (decimal)20),
                    Name = p.Fullname,
                    Id = p.Id,
                    PositionRate = p.Positions[position] ?? 1,
                    SideRate = position == Position.GoalKeeper ? 20 : (p.Sides[side] ?? 1)
                });
            }

            var totalRate = playersRatedList.Sum(pr => pr.AttributeRate);
            foreach (var pr in playersRatedList)
                pr.PercentOfTotal = (int)Math.Round(pr.AttributeRate / (decimal)totalRate);

            return playersRatedList
                .OrderByDescending(_ => _.AttributeRate)
                .ToList();
        }

        private Dictionary<Tuple<Position, Side>, List<PlayerRateItemData>> GetRatedPlayersForAll()
        {
            var ratedPlayersAllPos = new Dictionary<Tuple<Position, Side>, List<PlayerRateItemData>>();
            foreach (var p in Enum.GetValues(typeof(Position)).Cast<Position>())
            {
                foreach (var s in Enum.GetValues(typeof(Side)).Cast<Side>())
                {
                    ratedPlayersAllPos.Add(
                        new Tuple<Position, Side>(p, s),
                        GetRatedPlayers(p, s));
                }
            }
            return ratedPlayersAllPos;
        }
    }
}
