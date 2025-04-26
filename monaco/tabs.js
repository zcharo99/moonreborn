const tabsDiv = document.getElementById("tabs");
const tabsJson = fetch("tabs.json");
console.log(tabsJson);
const tabs = JSON.parse(tabsJson);

for(var k in result) {
    console.log(k, result[k]);
 }