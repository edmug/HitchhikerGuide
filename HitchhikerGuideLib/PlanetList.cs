﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using HitchhikerGuide.Data;

namespace HitchhikerGuide
{
    public class PlanetList : IEnumerable<Planet>
    {
        private readonly IRepository _repository;

        public PlanetList(IRepository repository)
        {
            _repository = repository;
            _planets = repository.LoadPlanets()
                .Select(planet => new Planet(planet, repository))
                .ToList();
        }

        private readonly List<Planet> _planets;
        private Planet _selectedItem;

        public IEnumerator<Planet> GetEnumerator()
        {
            return _planets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Planet SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem == value) return;

                _selectedItem = value;
                OnSelectedPlanetChanged();
            }
        }

        public event EventHandler<SelectedPlanetChangedEventArgs> SelectedPlanetChanged;

        private void OnSelectedPlanetChanged() => SelectedPlanetChanged?.Invoke(this, new SelectedPlanetChangedEventArgs(SelectedItem));


        public void Add()
        {
            var planet = new Planet(_repository);

            _planets.Add(planet);
            OnCollectionChanged(CollectionChangeReason.ItemAdded);

            SelectedItem = planet;
        }

        public void Delete()
        {
            var planet = SelectedItem;
            if (planet == null)
                throw new InvalidOperationException($"{nameof(SelectedItem)} must be set before deleting");

            var index = _planets.IndexOf(planet);

            _repository.Delete(planet);

            _planets.Remove(planet);
            OnCollectionChanged(CollectionChangeReason.ItemRemoved);

            SelectedItem = _planets.Skip(index).FirstOrDefault() ?? _planets.LastOrDefault();

        }

        /// <summary>
        /// Occurs when a planet is added or removed from this collection.
        /// </summary>
        public event EventHandler<CollectionChangedEventArgs> CollectionChanged;

        private void OnCollectionChanged(CollectionChangeReason reason) => CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(reason));
    }
}
