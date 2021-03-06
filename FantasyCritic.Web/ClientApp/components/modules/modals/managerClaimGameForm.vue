<template>
  <b-modal id="claimGameForm" ref="claimGameFormRef" title="Add Publisher Game" hide-footer @hidden="clearData">
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="searchGame">
      <div class="form-group">
        <label for="claimGameName" class="control-label">Game Name</label>
        <div class="input-group game-search-input">
          <input v-model="claimGameName" id="claimGameName" name="claimGameName" type="text" class="form-control input" />
          <span class="input-group-btn">
            <b-button variant="info" v-on:click="searchGame">Search Game</b-button>
          </span>
        </div>
        <possibleMasterGamesTable v-if="possibleMasterGames.length > 0" v-model="claimMasterGame" :possibleGames="possibleMasterGames" :maximumEligibilityLevel="maximumEligibilityLevel"></possibleMasterGamesTable>
        <div v-if="claimMasterGame">
          Selected Game: {{claimMasterGame.gameName}}
        </div>
      </div>
    </form>
    <form method="post" class="form-horizontal" role="form" v-on:submit.prevent="addGame">
      <div class="form-group">
        <label for="claimPublisher" class="control-label">Publisher</label>
        <b-form-select v-model="claimPublisher">
          <option v-for="publisher in publishers" v-bind:value="publisher">
            {{ publisher.publisherName }}
          </option>
        </b-form-select>
      </div>
      <div class="form-check">
        <span>
          <label class="form-check-label">
            CounterPick
          </label>
          <input class="form-check-input override-checkbox" type="checkbox" v-model="claimCounterPick">
        </span>
      </div>
      <div>
        <input type="submit" class="btn btn-primary add-game-button" value="Add game to publisher" v-if="formIsValid" />
      </div>
      <div v-if="claimResult && !claimResult.success" class="alert claim-error" v-bind:class="{ 'alert-danger': !claimResult.overridable, 'alert-warning': claimResult.overridable }">
        <h4 class="alert-heading" v-if="claimResult.overridable">Warning!</h4>
        <h4 class="alert-heading" v-if="!claimResult.overridable">Error!</h4>
        <ul>
          <li v-for="error in claimResult.errors">{{error}}</li>
        </ul>

        <div class="form-check" v-if="claimResult.overridable">
          <span>
            <label class="form-check-label">
              Do you want to override these warnings?
            </label>
            <input class="form-check-input override-checkbox" type="checkbox" v-model="claimOverride">
          </span>
        </div>
      </div>
    </form>
  </b-modal>
</template>

<script>
    import Vue from "vue";
    import axios from "axios";
    import PossibleMasterGamesTable from "components/modules/possibleMasterGamesTable";
    export default {
        data() {
            return {
                claimGameName: null,
                claimPublisher: null,
                claimMasterGame: null,
                claimGameType: null,
                claimResult: null,
                claimOverride: false,
                claimCounterPick: false,
                possibleMasterGames: []
            }
        },
        components: {
            PossibleMasterGamesTable
        },
        computed: {
          formIsValid() {
            return (this.claimGameName && this.claimPublisher);
          }
        },
        props: ['publishers', 'maximumEligibilityLevel'],
        methods: {
            searchGame() {
                axios
                    .get('/api/game/MasterGame?gameName=' + this.claimGameName)
                    .then(response => {
                        this.possibleMasterGames = response.data;
                    })
                    .catch(response => {

                    });
            },
            addGame() {
                var gameName = this.claimGameName;
                if (this.claimMasterGame !== null) {
                    gameName = this.claimMasterGame.gameName;
                }

                var masterGameID = null;
                if (this.claimMasterGame !== null) {
                    masterGameID = this.claimMasterGame.masterGameID;
                }

                var request = {
                    publisherID: this.claimPublisher.publisherID,
                    gameName: gameName,
                    counterPick: this.claimCounterPick,
                    masterGameID: masterGameID,
                    managerOverride: this.claimOverride
                };

                axios
                  .post('/api/leagueManager/ManagerClaimGame', request)
                  .then(response => {
                      this.claimResult = response.data;
                      if (!this.claimResult.success) {
                        return;
                      }
                      this.$refs.claimGameFormRef.hide();
                      var claimInfo = {
                        gameName,
                        publisherName: this.claimPublisher.publisherName
                      };
                      this.$emit('gameClaimed', claimInfo);
                      this.claimGameName = null;
                      this.claimPublisher = null;
                      this.claimMasterGame = null;
                      this.claimCounterPick = false;
                      this.claimOverride = false;
                      this.possibleMasterGames = [];
                    })
                    .catch(response => {
                      
                    });
            },
            clearData() {
              this.claimResult = null;
              this.claimGameName = null;
              this.claimPublisher = null;
              this.claimMasterGame = null;
              this.claimCounterPick = false;
              this.claimOverride = false;
              this.possibleMasterGames = [];
            }
        }
    }
</script>
<style scoped>
.add-game-button{
  width: 100%;
}
.claim-error{
  margin-top: 10px;
}
.game-search-input{
  margin-bottom: 15px;
}
.override-checkbox {
  margin-left: 10px;
  margin-top: 8px;
}
</style>
