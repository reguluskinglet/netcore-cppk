import './pager.scss'
import * as React from 'react'
import * as classnames from 'classnames';

const PaginationPrev = "«";
const PaginationNext = "»";

interface PagerHelperProps
{
    pager: Pager
    pagerText?: (pager: Pager) => string
    onPageSelected: (pagenumber: number) => void
}

export default class PagerHelper extends React.Component<PagerHelperProps, any> {

    createPageLinkPrevOrNext = (pageNumber: number, text: string) => {
        return <a onClick={() => this.props.onPageSelected(pageNumber)}><span>{text}</span></a>
    }

    createPageLinkDisabled = (text: string) => {
        return <li className="disabled"><span>{text}</span></li>
    }

    createPageLink = (pageNumber: number, text: any) => {
        if (pageNumber == this.props.pager.currentPage)
            return <span className="active">{text}</span>

        return this.createPageLinkPrevOrNext(pageNumber,text);
    }


    renderSides = () => {
        if (this.props.pager.hasSides) {

            var pages = [1, 2, 3].map((x, i) => <li key={i}>{this.createPageLink(x, x)}</li>)

            if (this.props.pager.firstPageInPagesFrame != 4)
                pages[3] = <li key={4}><a>...</a></li>

            return pages;
        }
    }

    renderPages=()=>{

        var result=[]

        for (var i = this.props.pager.firstPageInPagesFrame; i <= this.props.pager.lastPageInPagesFrame; i++)
        {
            result.push(<li key={i}>{this.createPageLink(i, i)}</li>)
		}

        return result;
    }

    renderLastSides=()=>{
         if (this.props.pager.hasSides) {

             var result=[]

             if (this.props.pager.lastPageInPagesFrame != (this.props.pager.totalPagesCount - 3))
                 result.push(<li key={'page-last'}><a>...</a></li>);

          //  var pages =  [1, 2, 3].map(x => this.createPageLink(x, x))

            for (var i = (this.props.pager.totalPagesCount-2); i <= this.props.pager.totalPagesCount; i++)
            {
                result.push(<li key={i}>{this.createPageLink(i, i)}</li>)
            }

            return result;
        }
    }

    getPageInfo = (pager) => {
        return pager.firstElementOnPage +' - '+ pager.lastElementOnPage + ' из ' + pager.totalElementsCount
    }

    get pagerText(): string {
        return 'Всего: '+ this.props.pager.totalElementsCount;
    }

    render() {

        var pager = this.props.pager;

        return (
            pager.totalPagesCount == 0 ? null :
                pager.totalPagesCount < 2 ? <nav className="page-nav"><span className="page-info">{this.props.pagerText ? this.props.pagerText(pager) : this.pagerText}</span></nav> :
                    <nav className="page-nav">
                        <span className="page-info">{this.props.pagerText ? this.props.pagerText(pager) :this.getPageInfo(pager)}</span>
                        <ul className="g-pagination">
                            {pager.currentPage == 1
                                ? this.createPageLinkDisabled(PaginationPrev)
                                : <li>{this.createPageLinkPrevOrNext(pager.currentPage - 1, PaginationPrev)}</li>}
                            {this.renderSides()}
                            {this.renderPages()}
                            {this.renderLastSides()}
                            {pager.currentPage==pager.totalPagesCount
                                ?this.createPageLinkDisabled(PaginationNext)
                                :<li>{this.createPageLinkPrevOrNext(pager.currentPage + 1, PaginationNext)}</li>}
                        </ul>
                    </nav>
        )
    }
}