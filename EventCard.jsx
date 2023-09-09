import React, { useState } from "react";
import { Button } from 'react-bootstrap';
import AddEvent from "./AddEvent";
import MoreDetails from "./MoreDetailsModal";



function EventCard(props) {

    const [moreOpen, setMoreOpen] = useState(false);
    const { event } = props;
    const [isOpen, setIsOpen] = useState(false);
    const handleEditEventClick = () => {
        setIsOpen(true);
    }

    const onClose = () => {
        setIsOpen(false);
    }
    const handleShowMoreModal = () => {
        setMoreOpen(true);
    }

    const handleCloseMoreModal = () => {
        setMoreOpen(false);
    }

    let dateStart = event.dateStart;
    let date = new Date(dateStart);

    let options = { year: "numeric", month: "long", day: "numeric", hour: "2-digit", minute: "2-digit", hour12: true };
    let formattedDate = date.toLocaleDateString("en-US", options);

    return (<>
        <div className="col nop" key={"ListE-" + event.id} >
            <div className="event-card">
                <div className="item"><strong>{event.name}</strong></div>
                <div className="item">{event.headline}</div>
                <div className="item">{formattedDate}</div>
                <div className="item">{event.summary}</div>
                <div className="cen">
                    <Button variant="primary" className="mar-5 more-btn" onClick={handleShowMoreModal}>More Details</Button>
                    <Button variant="warning" className="mar-5" onClick={handleEditEventClick}>Edit</Button>
                </div>
            </div> <AddEvent isOpen={isOpen} onClose={onClose} event={event} />
        </div>
        <MoreDetails key={event.id} event={event} moreOpen={moreOpen} handleCloseMoreModal={handleCloseMoreModal} />

    </>
    );
};

export default EventCard
