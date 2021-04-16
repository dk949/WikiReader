﻿using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.UI;
using WikiBrowser.Requests;
using WikiBrowser.UI.SpecialisedUIElements;
using static WikiBrowser.Logging;

namespace WikiBrowser.UI {
    // MainUIState's visibility is toggled by typing "/wiki" in chat. (See WikiCommand.cs)
    internal class MainUiState : UIState {
        public static bool Visible;

        private ArticleContainer _article;

        private DragableUiPanel _mainPanel;
        private TerrariaRequest _request;

        private VanillaItemSlotWrapper _itemSlot;

        public override void OnInitialize() {
            _request = new TerrariaRequest();

            _mainPanel = new MainPanel();


            var closeButton = new CloseButton(CloseButtonClicked);
            _mainPanel.Append(closeButton);


            var searchButton = new SearchButton(SearchButtonClicked);
            _mainPanel.Append(searchButton);


            var upButton = new UpButton(PageUpClicked);
            _mainPanel.Append(upButton);


            var downButton = new DownButton(PageDownClicked);
            _mainPanel.Append(downButton);


            _itemSlot = new SearchItemSlot();
            _mainPanel.Append(_itemSlot);


            _article = new ArticleContainer();
            _mainPanel.Append(_article);

            Append(_mainPanel);
        }

        private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
            Main.PlaySound(SoundID.MenuClose);
            ReturnItem();

            //TODO: make this part configurable?
            _article.UiTitle = "";
            _article.UiBody = "";
            _article.UiCurrentPage = 0;
            Visible = false;
        }

        internal void ReturnItem() {
            if (_itemSlot.Item.IsAir) return;
            // QuickSpawnClonedItem will preserve mod data of the item. QuickSpawnItem will just spawn a fresh version of the item, losing the prefix.
            Main.LocalPlayer.QuickSpawnClonedItem(_itemSlot.Item, _itemSlot.Item.stack);
            _itemSlot.Item.TurnToAir();
        }

        private void SearchButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
            Main.PlaySound(SoundID.MenuTick);
            _article.UiCurrentPage = 0;
            Log("Started request", LogType.Info);
            if (_itemSlot.Item.IsAir) {
                _article.UiTitle = "No Item";
                _article.UiBody = "";
                return;
            }

            PerformRequest(_itemSlot.Item.Name);
        }

        public void PerformRequest(string item) {
            _request.GetItem(item);
            Task.Run(() => {
                while (!_request.IsDone()) {
                    _article.UiBody = "Loading...";
                    _article.UiTitle = "";
                }

                _article.UiTitle = _request.Result().Title;
                _article.UiBody = _request.Result().Body;
                Log("Task finished, page loaded", LogType.Info);
            });
        }

        private void PageUpClicked(UIMouseEvent evt, UIElement listeningElement) {
            Main.PlaySound(SoundID.MenuTick);
            _article.PrevPage();
        }

        private void PageDownClicked(UIMouseEvent evt, UIElement listeningElement) {
            Main.PlaySound(SoundID.MenuTick);
            _article.NextPage();
        }
    }
}