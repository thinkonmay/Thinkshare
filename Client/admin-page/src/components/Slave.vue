<template>
  <b-modal id="addAnswerModal" ref="addAnswerModal" hide-footer title="Add new Answer" @hidden="onHidden">
    <b-form @submit.prevent="onSubmit" @reset.prevent="onCancel">
      <b-form-group label="Your Answer:" label-for="answerInput">
        <b-form-textarea id="answerInput"
                      v-model="form.body"
                      placeholder="Provide an answer"
                      :rows="6"
                      :max-rows="10">
        </b-form-textarea>
      </b-form-group>

      <button class="btn btn-primary float-right ml-2" type="submit" >Submit</button>
      <button class="btn btn-secondary float-right" type="reset">Cancel</button>
    </b-form>
  </b-modal>
</template>

<script>
export default {
  name: 'Slave',
  props: {
    msg: String
  },
  data(){
      SlaveList:[]      
  },
  methods:
  {
    onSlaveIdAdd(event){
        this.$http.get('/Admin/AddSlave/${SlaveID}', this.slaveId);},
    onSlaveReject(event){
        this.$http.post('/Admin/RejectSlave/${ID}',this.slaveId);
    }      
  },
  created (){
    this.$adminHub.$on('on-session-core-exit',this.onSessionCoreExit)
    this.$http.get('/Admin/System').then(res=>{
        this.SlaveList = read.data
    })
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
h3 {
  margin: 40px 0 0;
}
ul {
  list-style-type: none;
  padding: 0;
}
li {
  display: inline-block;
  margin: 0 10px;
}
a {
  color: #42b983;
}
</style>
