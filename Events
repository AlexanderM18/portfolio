import React, { useState, useEffect } from "react";
import { Form, Button } from "react-bootstrap";
import AddEvent from "./AddEvent";
import EventCard from "./EventCard";
import { eventsFeed, addEvent } from "../../services/eventService";
import { useLocation } from "react-router-dom";
import SearchEvents from "./SearchEvents";
import "./events.css";
import "rc-pagination/assets/index.css";
import Pagination from "rc-pagination";
import Loki from "react-loki";
import toastr from "toastr";
import UilFilm from "@iconscout/react-unicons/icons/uil-film";
import UilCalender from "@iconscout/react-unicons/icons/uil-calender";
import UilMap from "@iconscout/react-unicons/icons/uil-map";
import UilImage from "@iconscout/react-unicons/icons/uil-image";
import party from "../../services/party.mp4";
import debug from "debug";
import AddPart1 from "./AddPart1";
import AddPart2 from "./AddPart2";
import AddPart3 from "./AddPart3";
import AddPart4 from "./Addpart4";

function Events() {
  const [pageData, setPageData] = useState({
    eventsData: [],
    eventComponents: [],
    totalResults: 0,
  });
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState({ startDate: "", endDate: "" });
  const [searchButtonClicked, setSearchButtonClicked] = useState(false);
  const location = useLocation();
  const updatedEvent = location.state;
  const [currentPage, setCurrentPage] = useState(1);
  const [addEvt, setAddEvt] = useState({
    name: "",
    headline: "",
    description: "",
    summary: "",
    dateStart: "",
    dateEnd: "",
    latitude: undefined,
    longitude: undefined,
    address: "",
    city: "",
    state: "",
    zipCode: "",
    slug: "",
    statusId: "",
  });

  const mergeValues = (values) => {
    setAddEvt((prevState) => ({
      ...prevState,
      ...values,
    }));
  };

  const finishWizard = () => {
    const finalValues = {
      ...addEvt,
      location: [
        {
          latitude: addEvt.latitude,
          longitude: addEvt.longitude,
          address: addEvt.address,
          zipCode: addEvt.zipCode,
          city: addEvt.city,
          state: addEvt.state,
        },
      ],
    };
    console.log("finish form data: ", finalValues);
    addEvent(finalValues).then(onAddEventSuccess).catch(onAddEventError);

    function onAddEventSuccess(response) {
      console.log("response: ", response);

      toastr.success(`Event Added`);
      setPageData((prevState) => ({
        ...prevState,
        eventsData: [...prevState.eventsData, finalValues],
        eventComponents: [...prevState.eventsData, finalValues].map(mapEvents),
      }));
    }

    function onAddEventError(error) {
      console.log("error: ", error);
    }
  };

  const customSteps = [
    {
      label: "Step 1",
      icon: <UilFilm size="40" color="#61DAFB" />,
      component: <AddPart1 addEvt={addEvt} />,
    },
    {
      label: "Step 2",
      icon: <UilCalender size="40" color="#61DAFB" />,
      component: <AddPart2 addEvt={addEvt} />,
    },
    {
      label: "Step 3",
      icon: <UilMap size="40" color="#61DAFB" />,
      component: <AddPart3 addEvt={addEvt} />,
    },
    {
      label: "Step 4",
      icon: <UilImage size="40" color="#61DAFB" />,
      component: <AddPart4 addEvt={addEvt} />,
    },
  ];

  //#region
  const onChange = (page) => {
    setCurrentPage(page);
  };
  const pageIndex = currentPage - 1;
  const pageSize = 8;

  const handleAddEventClick = () => {
    setIsOpen(true);
  };

  const onClose = () => {
    setIsOpen(false);
  };

  useEffect(() => {
    eventsFeed(pageIndex, pageSize).then(onGetEventsSuccess).catch(onGetEventsError);
  }, [currentPage]);

  function onFormFieldChange(event) {
    event.preventDefault();
    const value = event.target.value;
    const name = event.target.name;
    setSearchTerm((prevState) => {
      let copy = { ...prevState };
      copy[name] = value;
      return copy;
    });
  }

  const sendTermToSearch = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setSearchButtonClicked(true);
  };

  const changeToFalse = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setSearchButtonClicked(false);
  };

  const onGetEventsSuccess = (data) => {
    let { pagedItems, totalCount } = data.data.item;
    const sortedEvents = pagedItems.sort((a, b) => {
      const dateA = new Date(a.date);
      const dateB = new Date(b.date);
      return dateA - dateB;
    });
    setPageData((prevState) => {
      const pd = { ...prevState };
      pd.eventsData = sortedEvents;
      pd.eventComponents = sortedEvents.map(mapEvents);
      pd.totalResults = totalCount;
      return pd;
    });
  };
  const onGetEventsError = (error) => {
    console.log("Error: ", error);
  };
  useEffect(() => {
    updateEvent(updatedEvent);
  }, [updatedEvent]);

  const updateEvent = (updatedEvent) => {
    setPageData((prevState) => {
      const newPageData = { ...prevState };
      newPageData.eventsData = [...newPageData.eventsData];
      const idxOf = prevState.eventsData.findIndex((curEvent) => {
        let result = false;
        if (curEvent.id === updatedEvent.id) {
          result = true;
        }
        return result;
      });
      if (idxOf >= 0) {
        newPageData.eventsData.splice(idxOf, 1, updatedEvent.payload);
      }
      newPageData.eventComponents = newPageData.eventsData.map(mapEvents);
      return newPageData;
    });
  };

  //here is where the events go in
  const mapEvents = (event) => {
    return (
      <EventCard
        key={event.id}
        event={event}
        setPageData={setPageData}
        mapEvents={mapEvents}
      />
    );
  };
  //#endregion
  return (
    <>
      <video autoPlay loop muted id="video">
        <source src={party} type="video/mp4" />
      </video>
      <div className="e-title-card tran">
        <h1 className="cen e-form-head">Events</h1>
      </div>
      <div>
        <div className="row cen">
          <div className="col-7">
            <div className="row e-cent container tran" id="add-evt-card">
              <Loki
                steps={customSteps}
                onNext={mergeValues.bind(this)}
                onBack={mergeValues.bind(this)}
                onFinish={finishWizard.bind(this)}
                noActions
              />
            </div>
            <div className="row tran" id="map-card">
              <iframe
                title="mapZilla"
                className="map"
                loading="lazy"
                allowFullScreen
                referrerPolicy="no-referrer-when-downgrade"
                src="https://www.google.com/maps/embed/v1/place?key=></iframe>
            </div>
          </div>
          <div className="col-4 marl-50">
            <div className="row evt-card marb-25 tran">
              <h3 className="cen">Search By Date:</h3>
              <div>
                <div className="Form">
                  <div className="search cen ">
                    <Form.Group>
                      <label htmlFor="startDate">
                        <h5>Start Date:</h5>
                      </label>
                      <input
                        type="date"
                        id="startDate"
                        name="startDate"
                        onChange={onFormFieldChange}
                      />
                    </Form.Group>
                    <Form.Group className="marl-20">
                      <label htmlFor="endDate" className="ml-10">
                        <h5>End Date:</h5>
                      </label>
                      <input
                        type="date"
                        id="endDate"
                        name="endDate"
                        onChange={onFormFieldChange}
                      />
                    </Form.Group>
                  </div>
                  <div className="text-center">
                    <Button
                      type="button"
                      className="btn btn-primary"
                      id="searchSubmit"
                      onClick={sendTermToSearch}>
                      Submit
                    </Button>
                  </div>
                </div>
              </div>
            </div>
            <div className="row evt-card2 tran">
              <div className="col mb-20 mt-20">
                <Button id="allMap" variant="warning" className="mar-5 more-btn">
                  View All On Map
                </Button>
              </div>
              <div className="col">
                <div className="align-r">
                  <Button
                    id="addEvent"
                    variant="warning"
                    className="mar-5 more-btn"
                    onClick={handleAddEventClick}>
                    Add Event
                  </Button>
                  <Button
                    id="viewAll"
                    variant="warning"
                    className="mar-5 more-btn"
                    onClick={changeToFalse}>
                    View All
                  </Button>
                </div>
              </div>
              <h3 className="mart-15">Upcoming Events</h3>
              <div>
                <Pagination
                  onChange={onChange}
                  currentPage={currentPage}
                  total={pageData.totalResults}
                />
              </div>
              <div className="clone-container " id="feed">
                {searchButtonClicked ? (
                  <SearchEvents searchTerm={searchTerm} />
                ) : (
                  pageData.eventComponents
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
      <AddEvent isOpen={isOpen} onClose={onClose} updateEvent={updateEvent} />
    </>
  );
}

export default Events;
