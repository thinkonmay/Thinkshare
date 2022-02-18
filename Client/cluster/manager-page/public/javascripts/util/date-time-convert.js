export function convertDate(data){
    // This function convert: "2022-02-01 12:12:00" -> "Tue Feb 01 2022 12:12:00 GMT+0700 (Indochina Time)" -> "2022-02-01T05:12:00.000Z"
    var date = data ? new Date(data): new Date("2022-01-01T12:00:00Z"); // check null
    return date ? date.toISOString() : "1990-01-01T00:00:00.000Z"; //will return an ISO representation of the date
}